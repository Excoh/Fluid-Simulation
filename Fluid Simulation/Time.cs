using System;
using Microsoft.Xna.Framework;

namespace Fluid_Simulation
{
    public static class Time
    {
        public static float elapsedGameTime { get; private set; }
        public static TimeSpan totalGameTime { get; private set; }
        public static float deltaTime { get; private set; }
        public static float totalTime { get; private set; }

        public static void Initialize()
        {
            elapsedGameTime = 0;
            deltaTime = 0;
            totalGameTime = new TimeSpan(0);
            totalTime = 0;
        }

        public static void Update(GameTime gameTime)
        {
            elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            deltaTime = elapsedGameTime;  // time between the last frame in seconds
            totalGameTime = gameTime.TotalGameTime;
            totalTime = (float)totalGameTime.TotalSeconds;
        }


    }
}
