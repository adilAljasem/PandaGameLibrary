using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.Audio;

namespace PandaGameLibrary.System;

public class PandaCore
{
    private double collisionTimer = 0;
    private const double CollisionInterval = 1.0 / 60.0; // 60 times per second

    private Task updateTask = Task.CompletedTask;
    private SpriteBatch spriteBatch1;
    internal CollisionSystem CollisionSystem { get; } = new CollisionSystem();
    internal RenderSystem RenderSystem { get; } = new RenderSystem();
    internal DebugSystem debugSystem;
    public static PandaCore Instance { get; } = new PandaCore();
    /// <summary>
    /// To Make PandaGameLibrary Work you need to set Game
    /// </summary>
    public Game Game { get; set; }
    /// <summary>
    /// GameObjectSystem Is Propertie To Add or Remove Gameobject
    /// </summary>
    public GameObjectSystem GameObjectSystem { get; } = new GameObjectSystem();
    /// <summary>
    /// AudioSystem Is Propertie To Manage Audio Like Sound Effect And Song
    /// </summary>
    public AudioSystem AudioSystem { get; set; } = new AudioSystem();
    public NetworkSystem NetworkSystem { get; } = new NetworkSystem();
    internal double UpdateTimeCollisions { get; private set; }
    public void LoadContent(string FontPath)
    {
        spriteBatch1 = new SpriteBatch(Game.GraphicsDevice);
        debugSystem = new DebugSystem(FontPath);
        debugSystem?.LoadContent(Game.Content);
        AudioSystem.Initialize(Game.Content);
    }
    /// <summary>
    /// Call this Method In Update Game Class
    /// </summary>
    public void Update(GameTime gameTime)
    {
        Helper.Update(gameTime);
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        // Accumulate the time for collision checks
        collisionTimer += deltaTime;

        // Run collision checks at fixed intervals

        if (updateTask.IsCompleted)
        {
            updateTask = Task.Run(delegate
            {
                UpdateTimeCollisions = CollisionSystem.CheckCollisions(GameObjectSystem.GetEnabledGameObjects(), gameTime);
            });
        }

        // Update other systems
        GameObjectSystem.UpdateGameObjects(gameTime);
        AudioSystem.Update(gameTime);
        NetworkSystem.Update(gameTime);
        debugSystem?.Update(gameTime, GameObjectSystem.GetEnabledGameObjects().ToList());

    }

    /// <summary>
    /// Call this Method In Drow Game Class
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        RenderSystem.Draw(spriteBatch, GameObjectSystem.GetEnabledGameObjects(), gameTime);
    }
    /// <summary>
    /// If you want To Enable Debug Draw This Method In Game Class
    /// </summary>
    public void DrawDeubg()
    {
        debugSystem?.Draw(spriteBatch1);
    }
}
