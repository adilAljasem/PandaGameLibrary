using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaGameLibrary.Components;
using System.Diagnostics;

namespace PandaGameLibrary.System
{
    public class DebugSystem
    {
        Process currentProcess;

        KeyboardState currentKeyboardState;
        KeyboardState _previousKeyboardState;
        private List<GameObject> EnableGameObjects { get; set; }
        List<ColliderComponent> allColliders = new List<ColliderComponent>();
        List<Component> allComponents = new List<Component>();
        // Timer variables
        private double elapsedTime = 0.0; // Time since last invocation
        private double elapsedTimeRender = 0.0; // Time since last invocation

        private const double TimeInterval = 0.5f; // 1 second in seconds
        private bool IsDebugEnabled;
        private int cpuUsage;

        public bool ShowAllCollidersOn { get; set; }
        public bool ShowDeepDebug { get; set; }
        private SpriteFont font;
        private string fontPath;
        private SpriteBatch SpriteBatch;

        private double _renderTimeMs;
        private double _updateTimeMs;

        private Stopwatch _stopwatchUpdate;
        private Stopwatch _stopwatchDraw;


        public DebugSystem(string fontPath)
        {
            this.fontPath = fontPath;
            _stopwatchUpdate = new Stopwatch();
            _stopwatchDraw = new Stopwatch();
        }

        private void ShowAllColliders()
        {
            // Create a snapshot of the current state of the collection
            var gameObjectsSnapshot = EnableGameObjects.ToList();
            List<ColliderComponent> allColliders = new List<ColliderComponent>();

            // Collect all colliders in one pass
            foreach (var gameObject in gameObjectsSnapshot)
            {
                allColliders.AddRange(gameObject.GetComponents<ColliderComponent>());
            }
            for (int i = 0; i < allColliders.Count; i++)
            {
                allColliders[i].ShowCollider = ShowAllCollidersOn;
            }
        }
      
        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>(fontPath);
            SpriteBatch = new SpriteBatch(PandaCore.Instance.Game.GraphicsDevice);
        }
        public void Update(GameTime gameTime, List<GameObject> allGameObjectsInTheGame)
        {

            EnableGameObjects = allGameObjectsInTheGame;
            if (IsKeyPressed(Keys.F1))
            {
                ShowAllCollidersOn = !ShowAllCollidersOn;
                ShowAllColliders();
            }
            if (IsKeyPressed(Keys.F2))
            {
                ShowDeepDebug = !ShowDeepDebug;
            }

            if (ShowAllCollidersOn) ShowAllColliders();

            if (ShowDeepDebug)
            {
                var gameObjectsSnapshot = EnableGameObjects.ToList();
                allColliders = new List<ColliderComponent>();
                allComponents = new List<Component>();
                // Collect all colliders in one pass
                foreach (var gameObject in gameObjectsSnapshot)
                {
                    allComponents.AddRange(gameObject.GetComponents());
                    allColliders.AddRange(gameObject.GetComponents<ColliderComponent>());
                }

            }

            // Update the keyboard states after processing the input
            _previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            elapsedTimeRender += gameTime.ElapsedGameTime.TotalSeconds;

            // If more than 1 second has passed, call the method
            if (elapsedTime >= TimeInterval)
            {
                currentProcess = Process.GetCurrentProcess();
                _updateTimeMs = _stopwatchUpdate.Elapsed.TotalMilliseconds;

                elapsedTime = 0.0; // Reset timer
            }
            _stopwatchUpdate.Restart();

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (ShowDeepDebug)
            {
                if (elapsedTimeRender >= TimeInterval)
                {
                    _renderTimeMs = _stopwatchDraw.Elapsed.TotalMilliseconds;

                    elapsedTimeRender = 0.0; // Reset timer
                }
                _stopwatchDraw.Restart();
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, Helper.GetScaleMatrix(1.5f, 1.5f));
                Helper.DrawTextWithOutline(spriteBatch, font, $"EnableObjects: {EnableGameObjects.Count}", new Vector2(170f, 12f), Color.White,Color.Black, 0.5f, new Vector2(),1,SpriteEffects.None,0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"Colliders: {allColliders.Count}", new Vector2(170f, 30f), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"Components: {allComponents.Count}", new Vector2(170f, 48f), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"AllObjects: {PandaCore.Instance.GameObjectSystem.GetAllGameObjects().Count}", new Vector2(170f, 66f), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"Memory Usage: {currentProcess?.WorkingSet64 / 1024 / 1024} MB", new Vector2(320f, 30f), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"Current Threads: {Process.GetCurrentProcess().Threads.Count}", new Vector2(320f, 12), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"RenderTime: {_renderTimeMs} MS", new Vector2(540f, 30), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"UpdateTime: {_updateTimeMs} MS", new Vector2(540f, 12), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);
                Helper.DrawTextWithOutline(spriteBatch, font, $"UpdateCollisionTime: {PandaCore.Instance.UpdateTimeCollisions} MS", new Vector2(750, 12), Color.White, Color.Black, 0.5f, new Vector2(), 1, SpriteEffects.None, 0);

                //spriteBatch.DrawString(font, $"Objects: {allGameObjects.Count}", new Vector2(200f, 12f), Color.White);
                //spriteBatch.DrawString(font, $"Colliders: {allColliders.Count}", new Vector2(200f, 30f), Color.White);
                //spriteBatch.DrawString(font, $"Components: {allComponents.Count}", new Vector2(200f, 48f), Color.White);
                //spriteBatch.DrawString(font, $"Memory Usage: {currentProcess?.WorkingSet64 / 1024 / 1024} MB", new Vector2(320f, 30f), Color.White);
                //spriteBatch.DrawString(font, $"Current Threads: {Process.GetCurrentProcess().Threads.Count}", new Vector2(320f, 12), Color.White);
                //spriteBatch.DrawString(font, $"RenderTime: {_renderTimeMs} MS", new Vector2(540f, 30), Color.White);
                //spriteBatch.DrawString(font, $"UpdateTime: {_updateTimeMs} MS", new Vector2(540f, 12), Color.White);
                //spriteBatch.DrawString(font, $"UpdateCollisionTime: {PandaCore.Instance.UpdateTimeCollisions} MS", new Vector2(750, 12), Color.White);

                spriteBatch.End();
            }

        }

        bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }
    }
}
