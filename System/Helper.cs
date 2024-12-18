using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection;

namespace PandaGameLibrary.System
{
    public static class Helper
    {
        static KeyboardState currentKeyboardState;
        static KeyboardState _previousKeyboardState;

        public static T DeepClone<T>(this T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            return (T)DeepCloneInternal(obj, new Dictionary<object, object>());
        }


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

        public static void DrawTextWithOutline(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color textColor,
            Color outlineColor, float outlineThickness, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
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
        private static object DeepCloneInternal(object obj, Dictionary<object, object> clonedObjects)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            if (type.IsArray)
            {
                return CloneArray((Array)obj, clonedObjects);
            }

            if (clonedObjects.ContainsKey(obj))
            {
                return clonedObjects[obj];
            }

            // Special handling for XNA/MonoGame types
            if (type == typeof(Texture2D) || type == typeof(SpriteFont) || type == typeof(Effect))
            {
                return obj; // These are typically shared resources and shouldn't be deep-cloned
            }

            object clone;
            try
            {
                clone = Activator.CreateInstance(type);
            }
            catch (MissingMethodException)
            {
                // If there's no parameterless constructor, return the original object
                // You might want to log this or handle it differently based on your needs
                return obj;
            }

            clonedObjects[obj] = clone;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldValue = field.GetValue(obj);
                var clonedValue = DeepCloneInternal(fieldValue, clonedObjects);
                field.SetValue(clone, clonedValue);
            }

            return clone;
        }

        private static object CloneArray(Array array, Dictionary<object, object> clonedObjects)
        {
            var clonedArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
            clonedObjects[array] = clonedArray;

            for (int i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                var clonedElement = DeepCloneInternal(element, clonedObjects);
                clonedArray.SetValue(clonedElement, i);
            }

            return clonedArray;
        }

    }

}
