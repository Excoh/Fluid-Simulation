using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fluid_Simulation
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Effect effect;
        Effect start;
        Effect advect;
        Effect divergence;
        Effect jacobi;
        Effect boundary;

        Model plane;

        Matrix world, projection, view;
        Vector3 cameraPos;
        Vector3 target;
        float distance;
        Vector2 mousePos;

        const int screenWidth = 600;
        const int screenHeight = 600;

        Texture2D colors;
        Texture2D veltex;

        RenderTarget2D renderTarget;    // used to swap textures for preparing to draw
        RenderTarget2D bufferTarget;    
        RenderTarget2D pressureTarget;  // pressure field, used for jacobi iterations
        RenderTarget2D velocity;        // velocity field, used for advection

        bool isPoint = false;
        float divergeval = 0;
        float divleft = 0;
        float divup = 0;
        float divdown = 0;
        // Define quad vertices
        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
        };

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = screenWidth;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = screenHeight;   // set this value to the desired height of your window
            graphics.ApplyChanges();

            InputManager.Initialize();
            Time.Initialize();

            renderTarget = new RenderTarget2D(graphics.GraphicsDevice, screenWidth, screenHeight, false, graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            bufferTarget = new RenderTarget2D(graphics.GraphicsDevice, screenWidth, screenHeight, false, graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            pressureTarget = new RenderTarget2D(graphics.GraphicsDevice, screenWidth, screenHeight, false, graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            start = Content.Load<Effect>("fluid");
            advect = Content.Load<Effect>("advect");
            divergence = Content.Load<Effect>("divergence");
            jacobi = Content.Load<Effect>("jacobi");
            boundary = Content.Load<Effect>("boundary");

            colors = Content.Load<Texture2D>("colors");
            veltex = Content.Load<Texture2D>("vel-up");

            cameraPos = new Vector3(0, 0, 1);
            target = Vector3.Zero;
            distance = Vector3.Distance(cameraPos, target);
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraPos, target, new Vector3(0, 1, 0));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1, 0.1f, 100);

            effect = start;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            // set the velocity field
            SceneToTexture(renderTarget, start, veltex);
            DrawScene(start, veltex);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            InputManager.Update();
            Time.Update(gameTime);

            if (InputManager.IsMouseHeld(0))
            {
                mousePos = new Vector2(Mouse.GetState().X / (float)screenWidth, Mouse.GetState().Y / (float)screenHeight);
                isPoint = true;
            }
            else
            {
                mousePos = new Vector2(0.5f, 0.5f);
                isPoint = false;
            }

            if (InputManager.IsKeyDown(Keys.D))
            {
                if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift))
                {
                    divergeval -= 0.01f;
                }
                else
                divergeval += 0.01f;
            }
            if (InputManager.IsKeyDown(Keys.W))
            {
                if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift))
                {
                    divup -= 0.01f;
                }
                else
                divup += 0.01f;
            }
            if (InputManager.IsKeyDown(Keys.A))
            {
                if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift))
                {
                    divleft-= 0.01f;
                }
                else
                divleft+= 0.01f;
            }
            if (InputManager.IsKeyDown(Keys.S))
            {
                if (InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift))
                {
                    divergeval -= 0.01f;
                }
                else
                divergeval += 0.01f;
            }


            base.Update(gameTime);
        }

        protected void SceneToTexture(RenderTarget2D renderWrite, Effect render, Texture2D tex)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect = render;
            GraphicsDevice.SetRenderTarget(renderWrite);
            //Console.WriteLine(effect.Name);
            effect.Parameters["tex"].SetValue(tex);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3
                );
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        protected void Jacobi(RenderTarget2D renderWrite, Effect render, Texture2D tex, Texture2D pressureTex)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect = render;
            GraphicsDevice.SetRenderTarget(renderWrite);
            //Console.WriteLine(effect.Name);
            effect.Parameters["tex"].SetValue(tex);
            effect.Parameters["pressure"].SetValue(tex);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3
                );
            }
            GraphicsDevice.SetRenderTarget(null);
        }
        protected void JacobiDraw(Effect render, Texture2D tex, Texture2D pressureTex)
        {
            effect = render;
            effect.Parameters["tex"].SetValue(tex);
            effect.Parameters["pressure"].SetValue(pressureTex);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3
                );
            }
        }

        void DrawScene(Effect render, Texture2D tex)
        {
            effect = render;
            effect.Parameters["tex"].SetValue(tex);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                if (isPoint)
                {
                    if (effect.Parameters["Point"] != null)
                        effect.Parameters["Point"].SetValue(mousePos);
                }
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3
                );
            }
        }

        void DrawDiverge(Effect render, Texture2D tex, float right, float left, float up, float down)
        {
            effect = render;
            effect.Parameters["divright"].SetValue(right);
            effect.Parameters["divleft"].SetValue(left);
            effect.Parameters["up"].SetValue(up);
            effect.Parameters["down"].SetValue(down);
            effect.Parameters["tex"].SetValue(tex);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                if (isPoint)
                {
                    if (effect.Parameters["Point"] != null)
                        effect.Parameters["Point"].SetValue(mousePos);
                }
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3
                );
            }
        }

        float frame = 0;
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Gray);
            frame += Time.deltaTime;

            // Start with basic colors grid to texture

            SceneToTexture(bufferTarget, start, renderTarget);
            DrawScene(start, renderTarget);

            SceneToTexture(renderTarget, advect, bufferTarget);
            DrawScene(advect, bufferTarget);

            SceneToTexture(bufferTarget, divergence, renderTarget);
            DrawScene(divergence, renderTarget);

            SceneToTexture(renderTarget, divergence, bufferTarget);
            DrawDiverge(divergence, bufferTarget, divergeval,divleft, divup, divdown );

            for (int i = 0; i < 35; i++)
            {
                // buffertarget currently has the divergence 
                //SceneToTexture(renderTarget, jacobi, bufferTarget);
                //DrawScene(jacobi, bufferTarget);

                Jacobi(renderTarget, jacobi, bufferTarget, pressureTarget);
                JacobiDraw(jacobi, renderTarget, renderTarget);

                //SceneToTexture(pressureTarget, jacobi, renderTarget);
                //DrawScene(jacobi, renderTarget);
                //JacobiDraw(jacobi, renderTarget, pressureTarget);
            }

            //SceneToTexture(renderTarget, boundary, bufferTarget);
            //DrawScene(boundary, bufferTarget);

            //if (frame > 0.2f)
            //{
            //    SceneToTexture(renderTarget, advect, bufferTarget);
            //    DrawScene(advect, bufferTarget);
            //}

            //if (frame > 0.4f)
            //{
            //    SceneToTexture(bufferTarget, divergence, renderTarget);
            //    DrawScene(divergence, renderTarget);
            //}

            //if (frame > 0.6f)
            //{
            //    //Jacobi(renderTarget, pressureTarget, jacobi, bufferTarget);
            //    SceneToTexture(renderTarget, jacobi, bufferTarget);
            //    DrawScene(jacobi, bufferTarget);
            //}

            //if (frame > 2.5) frame = 0;

            if (InputManager.IsKeyPressed(Keys.Space))
            {
                frame = 0;
                SceneToTexture(renderTarget, start, veltex);
                DrawScene(start, veltex);
            }

            base.Draw(gameTime);
        }
    }
}
