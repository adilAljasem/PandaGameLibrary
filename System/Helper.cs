using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public static void DrawTextWithOutline(SpriteBatch spriteBatch,SpriteFont font,string text,Vector2 position,Color textColor,Color outlineColor,float outlineThickness,Vector2 origin,
float scale,
    SpriteEffects effects,
    float layerDepth)
        {
            // Draw the outline
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        Vector2 offsetPosition = position + new Vector2(x * outlineThickness, y * outlineThickness);
                        spriteBatch.DrawString(font, text, offsetPosition, outlineColor, 0f, origin, scale, effects, layerDepth);
                    }
                }
            }

            // Draw the main text
            spriteBatch.DrawString(font, text, position, textColor, 0f, origin, scale, effects, layerDepth);
        }


    }
}
