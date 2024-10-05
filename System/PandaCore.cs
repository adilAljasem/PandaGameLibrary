using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.Audio;

namespace PandaGameLibrary.System;

public class PandaCore
{
    private double collisionTimer = 0;
    private const double CollisionInterval = 1.0 / 60.0; // 60 times per second

    private Task updateTask = Task.CompletedTask;
    public static PandaCore Instance { get; } = new PandaCore();
    public Game Game { get; set; }
    //public ContentManager contentManager { get; set; }
    //public GraphicsDevice graphicsDevice { get; set; }
    public GameObjectSystem GameObjectSystem { get; } = new GameObjectSystem();
    public CollisionSystem CollisionSystem { get; } = new CollisionSystem();
    public RenderSystem RenderSystem { get; } = new RenderSystem();
    public AudioSystem AudioSystem { get; set; } = new AudioSystem();
    public NetworkSystem NetworkSystem { get; } = new NetworkSystem();
    internal DebugSystem debugSystem;
    private SpriteBatch spriteBatch1;
    internal double UpdateTimeCollisions { get; private set; }
    public void LoadContent(string FontPath)
    {
        spriteBatch1 = new SpriteBatch(Game.GraphicsDevice);
        debugSystem = new DebugSystem(FontPath);
        debugSystem?.LoadContent(Game.Content);
    }

    public void Update(GameTime gameTime)
    {
        Helper.Update(gameTime);
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        // Accumulate the time for collision checks
        collisionTimer += deltaTime;

        // Run collision checks at fixed intervals
        if (collisionTimer >= CollisionInterval)
        {
            // Ensure collision checks only run 60 times per second
            collisionTimer -= CollisionInterval;

            if (updateTask.IsCompleted)
            {
                updateTask = Task.Run(delegate
                {
                    UpdateTimeCollisions = CollisionSystem.CheckCollisions(GameObjectSystem.GetEnabledGameObjects(), gameTime);
                });
            }

        }

        // Update other systems
        GameObjectSystem.UpdateGameObjects(gameTime);
        AudioSystem.Update(gameTime);
        NetworkSystem.Update(gameTime);
        debugSystem?.Update(gameTime, GameObjectSystem.GetEnabledGameObjects().ToList());

    }


    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderSystem.Draw(spriteBatch, GameObjectSystem.GetEnabledGameObjects(), gameTime);
    }
    public void DrawDeubg()
    {
        debugSystem?.Draw(spriteBatch1);
    }
}
