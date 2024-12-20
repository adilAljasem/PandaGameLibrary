using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.System;
using System;

namespace PandaGameLibrary.Components;

public class Render2D : Component
{
    private bool isThereAnimation;
    private SpriteEffects spriteEffect = SpriteEffects.None;
    public int Width { get; private set; }
    public int Height { get; private set; }
    private AnimationManagerComponent AnimationManagerComponent { get; } = new AnimationManagerComponent();
    public Texture2D Texture { get; private set; }
    public Vector2 Position { get; set; }
    //the animation have diffrent sourceRectangle var
    public Rectangle SourceRectangle { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Rotation { get; set; } = 0f;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    //public float Scale { get; set; } = 1f;
    public Vector2 Scale { get; set; } = new Vector2(1, 1);
    public float LayerDepth { get; set; } = 1f;
    private SpriteColorEffect? SpriteColorEffect { get; set; }
    private CircleData circle;
    internal bool ShowCircleCollider { get; set; }
    internal float ColliderRadius { get; set; }
    internal Vector2 ColliderCenter { get; set; }
    private bool Stretched = false;
    private bool CircleDataCreated = false;
    private Texture2D _lineTexture;
    private bool _drawingLine { get; set; }
    Vector2 _detla;
    float _angle;
    Vector2 _start;
    private Color _color;
    private List<LineData> _linesToDraw = new List<LineData>();

    private class LineData
    {
        public Vector2 Start;
        public Vector2 End;
        public Color Color;
        public float Thickness;

        public LineData(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Start = start;
            End = end;
            Color = color;
            Thickness = thickness;
        }
    }

    public override void Awake()
    {
        Position = base.gameObject.Transform.Position;
        Rotation = base.gameObject.Transform.Rotation;
        SpriteColorEffect = new SpriteColorEffect(this);
        circle = new CircleData(Color);
        CreateLineTexture();
        //PandaCore.Instance.RenderSystem.AddGameObject(gameObject);
    }

    public Texture2D LoadTexture(string path, int width, int height)
    {
        Texture = PandaCore.Instance.Game.Content.Load<Texture2D>(path);
        AnimationManagerComponent.SetFrameSizeAnimation(width, height);
        Width = width;
        Height = height;

        return Texture;
    }

    public void LoadTexture(Texture2D texture2D, int width, int height)
    {
        Texture = texture2D;
        AnimationManagerComponent.SetFrameSizeAnimation(width, height);
        Width = width;
        Height = height;
    }

    public void HitEffect(Color effectColor, float effectDuration)
    {
        SpriteColorEffect?.TriggerEffect(effectColor, effectDuration);
    }

    public void PlayAnimation(string name)
    {
        AnimationManagerComponent.PlayAnimation(name);
    }

    public Animation AddAnimation(string AnimationName, Animation animation)
    {
        AnimationManagerComponent.AddAnimation(AnimationName, animation);
        isThereAnimation = true;
        return animation;
    }

    public Animation GetAnimation(string name)
    {
        return AnimationManagerComponent.GetAnimation(name);
    }

    public void FlipSprite(bool shouldFaceLeft)
    {
        spriteEffect = (shouldFaceLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
    }

    public void SetSpritRectangle(int index, float scale = 1.0f)
    {
        // Calculate the original sprite dimensions
        int originalSpriteWidth = Width;
        int originalSpriteHeight = Height;

        // Apply the scale to the sprite dimensions
        int scaledSpriteWidth = (int)(originalSpriteWidth * scale);
        int scaledSpriteHeight = (int)(originalSpriteHeight * scale);

        // Determine the column and row based on the index
        if (Texture != null)
        {
            int columns = Texture.Width / originalSpriteWidth;
            int row = index / columns;
            int column = index % columns;

            // Calculate the position within the texture for the original dimensions
            int x = column * originalSpriteWidth;
            int y = row * originalSpriteHeight;

            // Adjust the position to keep the scaling centered
            int adjustedX = x - (scaledSpriteWidth - originalSpriteWidth) / 2;
            int adjustedY = y - (scaledSpriteHeight - originalSpriteHeight) / 2;

            // Set the SourceRectangle with the scaled dimensions
            SourceRectangle = new Rectangle(adjustedX, adjustedY, scaledSpriteWidth, scaledSpriteHeight);
        }
    }

    internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Texture != null && !AnimationManagerComponent.IsPlaying && !Stretched)
        {
            spriteBatch.Draw(Texture, Position, SourceRectangle, Color, Rotation, Origin, Scale, spriteEffect, LayerDepth);
        }
        else
        {
            AnimationManagerComponent.Draw(spriteBatch, Position, Origin, Scale, Color, Rotation, LayerDepth, spriteEffect);
        }

        if (ShowCircleCollider)
        {
            circle.Draw(spriteBatch);
        }

        // Draw all lines
        foreach (var line in _linesToDraw)
        {
            Vector2 delta = line.End - line.Start;
            float angle = (float)Math.Atan2(delta.Y, delta.X);

            spriteBatch.Draw(
                _lineTexture,
                line.Start,
                null,
                line.Color,
                angle,
                Vector2.Zero,
                new Vector2(delta.Length(), line.Thickness),
                SpriteEffects.None,
                LayerDepth
            );
        }

        // Clear the lines after drawing
        _linesToDraw.Clear();
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        if (_lineTexture == null)
        {
            CreateLineTexture();
        }

        _linesToDraw.Add(new LineData(start, end, color, thickness));
    }
    private void UpdateCircle()
    {
        if (!CircleDataCreated) circle = new CircleData(Color);
        CircleDataCreated = true;
        circle.SetCircleData(ColliderCenter, ColliderRadius, 16);
        circle.Update();
    }

    public override void Update(GameTime gameTime)
    {
        AnimationManagerComponent.Update(gameTime);
        SpriteColorEffect?.Update(gameTime);
        if (ShowCircleCollider)
        {
            UpdateCircle();
        }
    }
    private void CreateLineTexture()
    {
        _lineTexture = new Texture2D(PandaCore.Instance.Game.GraphicsDevice, 1, 1);
        _lineTexture.SetData(new[] { Color.White });
    }
}
