using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PandaGameLibrary.System
{
    public class Animation
    {
        public Texture2D SpriteSheet { get; set; }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public int CurrentFrame { get; set; }
        public float FrameTime { get; set; }
        public float Timer { get; set; }
        public int FrameWidth { get; set; } = 32;
        public int FrameHeight { get; set; } = 32;
        private bool Looping { get; set; }
        public bool AnimationEnd { get; private set; } = false;
        private Rectangle currentSourceRectangle; // Pre-calculated source rectangle for drawing


        public Animation(Texture2D spriteSheet, int startFrame, int endFrame, float frameTime, bool looping = true)
        {
            SpriteSheet = spriteSheet;
            StartFrame = startFrame;
            EndFrame = endFrame;
            CurrentFrame = StartFrame;
            FrameTime = frameTime;
            Looping = looping;
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentFrame >= EndFrame && !Looping)
            {
                AnimationEnd = true;
                return;
            }

            Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Timer > FrameTime)
            {
                Timer = 0f;
                CurrentFrame++;

                if (CurrentFrame > EndFrame)
                {
                    if (Looping)
                    {
                        CurrentFrame = StartFrame;
                    }
                    else
                    {
                        CurrentFrame = EndFrame;
                    }
                }
            }
            UpdateSourceRectangle();

        }

        public void SetFrameSize(int width, int height)
        {
            FrameWidth = width;
            FrameHeight = height;
        }

        public Rectangle GetFirstFrame()
        {
            int row = StartFrame / (SpriteSheet.Width / FrameWidth);
            int column = StartFrame % (SpriteSheet.Width / FrameWidth);
            return new Rectangle(column * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
        }

        // Update the source rectangle based on the current frame
        private void UpdateSourceRectangle()
        {
            int row = CurrentFrame / (SpriteSheet.Width / FrameWidth);
            int column = CurrentFrame % (SpriteSheet.Width / FrameWidth);
            currentSourceRectangle = new Rectangle(column * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
        }

        // Draw method is now simpler since calculations are done in Update
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 origin, float scale, float rotation, float layer, SpriteEffects spriteEffect)
        {
            spriteBatch.Draw(SpriteSheet, position, currentSourceRectangle, color, rotation, origin, scale, spriteEffect, layer);
        }
    }
}
