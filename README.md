# PandaGameLibrary
 ![pa](https://github.com/user-attachments/assets/2b77c97d-a339-4e0c-9550-17432a1cab40)
 
  PandaGameLibrary is a simple library to Create 2D Games for [MonoGame](https://monogame.net/). the main idea of this library to use ECS simaller to unity also it has Collision System and Networking using Asp.net Signalr



# PandaGameLibrary Documentation

This document provides a comprehensive guide on how to use the PandaGameLibrary, a component-based game development framework built on MonoGame.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Core Concepts](#core-concepts)
3. [Game Objects and Components](#game-objects-and-components)
4. [Rendering](#rendering)
5. [Animation System](#animation-system)
6. [Collision Detection](#collision-detection)
7. [Audio System](#audio-system)
8. [Debugging Tools](#debugging-tools)
9. [Network Support](#network-support)
10. [Example Projects](#example-projects)

## Getting Started

### Installation

Add the PandaGameLibrary to your MonoGame project as a reference.

### Setting Up Your Game

In your main Game class, initialize the PandaCore system:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.System;

public class MyGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public MyGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        // Initialize PandaCore
        PandaCore.Instance.Game = this;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Load content for PandaCore systems
        PandaCore.Instance.LoadContent("Fonts/DebugFont"); // Path to your debug font
    }

    protected override void Update(GameTime gameTime)
    {
        // Update all PandaCore systems
        PandaCore.Instance.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Start spriteBatch for your own drawing
        _spriteBatch.Begin();
        // Your drawing code here
        _spriteBatch.End();
        
        // Draw PandaCore game objects
        PandaCore.Instance.Draw(_spriteBatch, gameTime);
        
        // Optional: Draw debug information
        PandaCore.Instance.DrawDeubg();
        
        base.Draw(gameTime);
    }
}
```

## Core Concepts

PandaGameLibrary follows a component-based architecture, similar to Unity:

- **GameObject**: The base entity in your game which can have components attached to it
- **Component**: Reusable modules that define behavior or properties
- **PandaCore**: The main system that coordinates all subsystems

## Game Objects and Components

### Creating Game Objects

```csharp
// Create a basic game object
GameObject player = new GameObject();
player.GameObjectName = "Player";
player.Tag = "player";

// Position the game object
player.Transform.Position = new Vector2(100, 100);
player.Transform.Rotation = 0.0f;
player.Transform.Scale = new Vector2(1.0f, 1.0f);
```

### Parent-Child Relationships

```csharp
// Create a parent object
GameObject parentObject = new GameObject();
parentObject.GameObjectName = "Parent";

// Create a child object
GameObject childObject = new GameObject();
childObject.GameObjectName = "Child";

// Add child to parent
parentObject.AddChild(childObject);

// Remove child from parent
parentObject.RemoveChild(childObject);
```

### Adding Components

```csharp
// Add a component to a game object
Render2D renderer = player.AddComponent<Render2D>();

// Or create component first and then add it
ColliderComponent collider = new ColliderComponent();
player.AddComponent(collider);

// Get a component from a game object
Render2D playerRenderer = player.GetComponent<Render2D>();

// Get all components of a specific type
List<ColliderComponent> colliders = player.GetComponents<ColliderComponent>();

// Remove a component
player.RemoveComponent<Render2D>();
```

### Enabling/Disabling Game Objects

```csharp
// Enable or disable a game object and all its children
player.SetEnable(false);
player.SetEnable(true);
```

## Rendering

### Basic Rendering

```csharp
// Create a game object with a renderer
GameObject sprite = new GameObject();
Render2D renderer = sprite.AddComponent<Render2D>();

// Load and set a texture
Texture2D texture = renderer.LoadTexture("Images/Sprite", 32, 32);
// Or if you already have a Texture2D
renderer.LoadTexture(myTexture, 32, 32);

// Set rendering properties
renderer.Color = Color.White;
renderer.Rotation = 0.0f;
renderer.Scale = new Vector2(2.0f, 2.0f);
renderer.LayerDepth = 0.5f; // 0 is front, 1 is back
```

### Drawing Lines

```csharp
// Get the renderer component
Render2D renderer = gameObject.GetComponent<Render2D>();

// Draw a line from point A to point B
renderer.DrawLine(
    new Vector2(0, 0), // Start point
    new Vector2(100, 100), // End point
    Color.Red, // Color
    2.0f // Thickness
);
```

## Animation System

### Creating and Using Animations

```csharp
// Create a game object with a renderer
GameObject character = new GameObject();
Render2D renderer = character.AddComponent<Render2D>();

// Load the sprite sheet
Texture2D spriteSheet = renderer.LoadTexture("Images/CharacterSpriteSheet", 32, 32);

// Create an animation
Animation walkAnimation = new Animation(
    spriteSheet, // Sprite sheet
    0,           // Start frame
    7,           // End frame
    0.1f,        // Frame time (seconds per frame)
    true         // Looping
);

// Add the animation to the renderer
renderer.AddAnimation("Walk", walkAnimation);

// Create another animation
Animation idleAnimation = new Animation(
    spriteSheet,
    8,
    11,
    0.2f,
    true
);
renderer.AddAnimation("Idle", idleAnimation);

// Play an animation
renderer.PlayAnimation("Walk");

// Check if an animation exists and get it
Animation anim = renderer.GetAnimation("Walk");
if (anim != null)
{
    // Do something with the animation
}
```

### Sprite Effects

```csharp
// Flip the sprite horizontally
renderer.FlipSprite(true);

// Apply a color effect/flash
renderer.HitEffect(Color.Red, 0.2f); // Red flash for 0.2 seconds
```

## Collision Detection

### Basic Colliders

```csharp
// Create a game object
GameObject player = new GameObject();
player.Transform.Position = new Vector2(100, 100);

// Add a collider component
ColliderComponent collider = player.AddComponent<ColliderComponent>();
collider.Radius = 20.0f; // Set the collision radius
collider.IsDynamic = true; // This object can move/be pushed

// Set up collision callbacks
collider.OnCollision += (other) => {
    // Called every frame while colliding
    Console.WriteLine($"Colliding with {other.GameObjectName}");
};

collider.OnEnterCollision += (other) => {
    // Called when collision begins
    Console.WriteLine($"Started colliding with {other.GameObjectName}");
};

collider.OnExitCollision += (other) => {
    // Called when collision ends
    Console.WriteLine($"Stopped colliding with {other.GameObjectName}");
};

// Visual debugging
collider.ShowCollider = true; // Show collision shape
collider.Color = Color.Red; // Set debug visualization color
```

### Collision Types

```csharp
// Create a static collider (doesn't move during collisions)
ColliderComponent wallCollider = wall.AddComponent<ColliderComponent>();
wallCollider.IsDynamic = false;

// Create a transparent collider (detects but doesn't resolve collisions)
ColliderComponent triggerCollider = trigger.AddComponent<ColliderComponent>();
triggerCollider.Transparent = true;
```

## Audio System

### Playing Sound Effects

```csharp
// Load a sound effect
PandaCore.Instance.AudioSystem.LoadSoundEffect("Jump", "Audio/Jump");

// Play the sound effect
PandaCore.Instance.AudioSystem.PlaySoundEffect("Jump");

// Play with volume control
PandaCore.Instance.AudioSystem.PlaySoundEffect("Jump", volume: 0.7f);
```

### Playing Music

```csharp
// Load a song
PandaCore.Instance.AudioSystem.LoadSong("MainTheme", "Audio/MainTheme");

// Play the song (looping by default)
PandaCore.Instance.AudioSystem.PlaySong("MainTheme");

// Play without looping
PandaCore.Instance.AudioSystem.PlaySong("MainTheme", repeat: false);

// Stop the current song
PandaCore.Instance.AudioSystem.StopSong();
```

### Audio Zones

```csharp
// Create a game object for the audio zone
GameObject audioZoneObject = new GameObject();
audioZoneObject.Transform.Position = new Vector2(300, 300);

// Add an audio zone component
AudioZone audioZone = audioZoneObject.AddComponent<AudioZone>();

// Initialize the audio zone
audioZone.InitializeAudioZone(
    "Audio/AmbientSound", // Content path
    150.0f,               // Radius of the zone
    "ForestAmbience",     // Audio name reference
    true,                 // Is it a song (true) or sound effect (false)?
    true                  // Is it dynamic (volume changes with distance)?
);

// Add a sound effect to the audio zone
audioZone.AddSoundEffect("Audio/BirdChirp", "BirdSound");

// Set volume for this zone
audioZone.ZoneAudioVolume = 0.8f;
audioZone.ZoneMusicVolume = 0.5f;

// Play a specific sound effect in the zone
audioZone.PlaySoundEffect("BirdSound");
```

### Global Audio Settings

```csharp
// Set global audio volumes
PandaCore.Instance.AudioSystem.SetAudioVolume(0.8f); // Sound effects
PandaCore.Instance.AudioSystem.SetMusicVolume(0.5f); // Music
```

## Debugging Tools

```csharp
// The Debug System is automatically initialized with PandaCore

// In-game controls:
// F1 - Toggle collider visualization for all objects
// F2 - Toggle detailed debug information
```

## Network Support

```csharp
// Connect to a SignalR hub
await PandaCore.Instance.NetworkSystem.StartConnectionAsync("https://yourgameserver.com/gamehub");

// Measure network latency
long ping = await PandaCore.Instance.NetworkSystem.MeasurePingAsync();
Console.WriteLine($"Current ping: {ping}ms");
```

## Example Projects

### Basic Character Controller

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PandaGameLibrary.Components;
using PandaGameLibrary.System;

public class PlayerController : Component
{
    private float moveSpeed = 200.0f;
    private Render2D renderer;
    private ColliderComponent collider;
    
    public override void Awake()
    {
        // Initialize components
        renderer = gameObject.GetComponent<Render2D>();
        if (renderer == null)
            renderer = gameObject.AddComponent<Render2D>();
            
        collider = gameObject.GetComponent<ColliderComponent>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<ColliderComponent>();
            collider.Radius = 20.0f;
            collider.IsDynamic = true;
        }
        
        // Load player sprite and animations
        Texture2D spriteSheet = renderer.LoadTexture("Images/Player", 32, 32);
        
        // Add animations
        Animation walkRight = new Animation(spriteSheet, 0, 3, 0.1f, true);
        renderer.AddAnimation("WalkRight", walkRight);
        
        Animation walkLeft = new Animation(spriteSheet, 4, 7, 0.1f, true);
        renderer.AddAnimation("WalkLeft", walkLeft);
        
        Animation idle = new Animation(spriteSheet, 8, 9, 0.5f, true);
        renderer.AddAnimation("Idle", idle);
        
        // Set default animation
        renderer.PlayAnimation("Idle");
    }
    
    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 movement = Vector2.Zero;
        
        // Get keyboard state
        KeyboardState keyboard = Keyboard.GetState();
        
        // Handle movement
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            movement.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            movement.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
        {
            movement.X -= 1;
            renderer.FlipSprite(true);
            if (movement != Vector2.Zero)
                renderer.PlayAnimation("WalkLeft");
        }
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
        {
            movement.X += 1;
            renderer.FlipSprite(false);
            if (movement != Vector2.Zero)
                renderer.PlayAnimation("WalkRight");
        }
        
        // Normalize and apply movement
        if (movement != Vector2.Zero)
        {
            movement.Normalize();
            gameObject.Transform.Position += movement * moveSpeed * deltaTime;
        }
        else
        {
            renderer.PlayAnimation("Idle");
        }
    }
}
```

### Game Initialization with Example Scene

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.Components;
using PandaGameLibrary.System;
using System;

public class MyGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public MyGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        // Initialize PandaCore
        PandaCore.Instance.Game = this;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Load content for PandaCore systems
        PandaCore.Instance.LoadContent("Fonts/DebugFont");
        
        // Create a simple scene
        CreateGameScene();
    }
    
    private void CreateGameScene()
    {
        // Create player
        GameObject player = new GameObject();
        player.GameObjectName = "Player";
        player.Tag = "player";
        player.Transform.Position = new Vector2(640, 360);
        
        // Add renderer
        Render2D playerRenderer = player.AddComponent<Render2D>();
        playerRenderer.LoadTexture("Images/Player", 32, 32);
        
        // Add collider
        ColliderComponent playerCollider = player.AddComponent<ColliderComponent>();
        playerCollider.Radius = 16.0f;
        playerCollider.IsDynamic = true;
        
        // Add custom controller
        player.AddComponent<PlayerController>();
        
        // Create walls
        CreateWall(100, 100, 500, 20); // Top
        CreateWall(100, 100, 20, 500); // Left
        CreateWall(100, 600, 500, 20); // Bottom
        CreateWall(600, 100, 20, 520); // Right
        
        // Create an audio zone
        GameObject audioZoneObj = new GameObject();
        audioZoneObj.Transform.Position = new Vector2(350, 350);
        AudioZone audioZone = audioZoneObj.AddComponent<AudioZone>();
        audioZone.InitializeAudioZone("Audio/Background", 200.0f, "BackgroundMusic", true, true);
    }
    
    private void CreateWall(float x, float y, float width, float height)
    {
        GameObject wall = new GameObject();
        wall.GameObjectName = "Wall";
        wall.Tag = "wall";
        wall.Transform.Position = new Vector2(x + width/2, y + height/2);
        
        // Add renderer
        Render2D wallRenderer = wall.AddComponent<Render2D>();
        wallRenderer.LoadTexture("Images/Wall", (int)width, (int)height);
        
        // Add collider
        ColliderComponent wallCollider = wall.AddComponent<ColliderComponent>();
        wallCollider.Radius = Math.Max(width, height) / 2;
        wallCollider.IsDynamic = false; // Static collider
    }

    protected override void Update(GameTime gameTime)
    {
        // Update all PandaCore systems
        PandaCore.Instance.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Begin basic sprite batch
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        // Draw all PandaCore game objects
        PandaCore.Instance.Draw(_spriteBatch, gameTime);
        
        _spriteBatch.End();
        
        // Draw debug info
        PandaCore.Instance.DrawDeubg();
        
        base.Draw(gameTime);
    }
}
```
