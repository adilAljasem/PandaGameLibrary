using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaGameLibrary.System
{
    public class Helper
    {
        static KeyboardState currentKeyboardState;
        static KeyboardState _previousKeyboardState;

        public static void Update(GameTime gameTime)
        {
            _previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

        }
        public static bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }
        public static Matrix GetScaleMatrix(float multiplayX, float multiplayY)
        {
            float scaleX = (float)PandaCore.Instance.Game.GraphicsDevice.Viewport.Width / (float)1920;
            float scaleY = (float)PandaCore.Instance.Game.GraphicsDevice.Viewport.Height / (float)1080;
            return Matrix.CreateScale(scaleX * multiplayX, scaleY * multiplayY, 1f);
        }
    }
}
