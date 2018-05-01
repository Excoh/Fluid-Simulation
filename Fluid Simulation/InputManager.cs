using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Fluid_Simulation
{
    public static class InputManager
    {
        static KeyboardState PreviousKeyboardState { get; set; }
        public static KeyboardState CurrentKeyboardState { get; set; }
        static MouseState PreviousMouseState { get; set; }
        public static MouseState CurrentMouseState { get; set; }

        public static void Initialize()
        {
            PreviousKeyboardState = CurrentKeyboardState =
                Keyboard.GetState();
            PreviousMouseState = CurrentMouseState =
                Mouse.GetState();
        }

        public static void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
        }

        public static bool IsMousePressed(int button)
        {
            switch (button)
            {
                case 0:
                    return PreviousMouseState.LeftButton == ButtonState.Pressed &&
                            CurrentMouseState.LeftButton == ButtonState.Released;
                case 1:
                    return PreviousMouseState.RightButton == ButtonState.Pressed &&
                           CurrentMouseState.RightButton == ButtonState.Released;
                case 2:
                    return PreviousMouseState.MiddleButton == ButtonState.Pressed &&
                           CurrentMouseState.MiddleButton == ButtonState.Released;
                default:
                    return false;
            }
        }

        public static bool IsMouseHeld(int button)
        {
            switch (button)
            {
                case 0:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return CurrentMouseState.RightButton == ButtonState.Pressed;
                case 2:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                default:
                    return false;

            }
        }

        public static bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        public static bool IsKeyUp(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) &&
                PreviousKeyboardState.IsKeyUp(key);
        }

        public static bool IsKeyReleased(Keys key)
        {
            return PreviousKeyboardState.IsKeyDown(key) &&
                CurrentKeyboardState.IsKeyUp(key);
        }

        public static Vector2 GetMousePosition()
        {
            return new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
        }
        public static Vector2 MousePosition
        {
            get
            {
                return new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
            }
        }
    }
}
