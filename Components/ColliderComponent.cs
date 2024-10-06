using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.System;
using System.Collections.Concurrent;

namespace PandaGameLibrary.Components
{
    public class ColliderComponent : Component
    {
        private CircleData circle = new CircleData();
        public Render2D Render2D { get; set; }
        public bool ShowCollider { get; set; } = false;
        public bool ColliderShapeCircle { get; set; } = true;
        public bool Transparent { get; set; }
        public bool TransparentWithStatic { get; set; }

        public Rectangle Bounds { get; private set; }

        public Vector2 Center { get; set; }
        public float Radius { get; set; } = 10f;
        private bool TileMapOptomaiztion { get; set; } = false;
        public bool Collision { get; set; } = true;
        private float XPosition { get; set; }
        private float YPosition { get; set; }
        private float imageWidth;
        private float imageHeight;

        public float Width { get; set; }
        public float Height { get; set; }

        public bool IsDynamic { get; set; } = false;

        public Vector2 Velocity { get; set; }

        public ConcurrentDictionary<Guid, byte> CollidingObjects { get; private set; } = new ConcurrentDictionary<Guid, byte>();

        public event Action<GameObject> OnCollision;
        public event Action<GameObject> OnEnterCollision;
        public event Action<GameObject> OnExitCollision;

        public ColliderComponent()
        {
        }

        //public float OtherColliderDistance(ColliderComponent colliderComponent)
        //{

        //}

        public void OnCollide(GameObject e)
        {
            OnCollision?.Invoke(e);
        }

        public void OnEnter(GameObject e)
        {
            OnEnterCollision?.Invoke(e);
        }

        public void OnExit(GameObject e)
        {
            OnExitCollision?.Invoke(e);
        }

        public override void Awake()
        {
            Render2D = gameObject.AddComponent<Render2D>();
        }

        public override void Start()
        {
            SetRadiusForTileMap();
        }

        private void SetRadiusForTileMap()
        {
            if (TileMapOptomaiztion)
            {
                float colliderDiameterX = Width / 2f;
                float colliderDiameterY = Height / 2f;
                Vector2 middleObject = base.gameObject.Transform.Position;
                float raudisSize = Math.Max(colliderDiameterX, colliderDiameterY);
                Radius = raudisSize;
            }
        }

        public void DrawColliderFromTiled(float xPosition, float yPosition, float ImageWidth, float ImageHeight)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            imageHeight = ImageHeight;
            imageWidth = ImageWidth;
            TileMapOptomaiztion = true;
        }

        public override void Update(GameTime gameTime)
        {
            //UpdateCircle();
            UpdateColliderCenterTileMap();
            UpdateGameObjectPosition(gameTime);
            if (ShowCollider)
            {
                Render2D.ShowCircleCollider = true;
                Render2D.ColliderRadius = Radius;
                Render2D.ColliderCenter = Center;
            }
            else
            {
                Render2D.ShowCircleCollider = false;
            }
        }

        public void UpdateColliderCenterTileMap()
        {
            ////int width = (int)base.gameObject.Transform.Scale.X;
            ////int height = (int)base.gameObject.Transform.Scale.Y;
            if (TileMapOptomaiztion)
            {
                float colliderDiameterX = Width / 2f;
                float colliderDiameterY = Height / 2f;
                Vector2 middleObject = base.gameObject.Transform.Position;
                Center = new Vector2(middleObject.X - imageWidth + XPosition + colliderDiameterX, middleObject.Y - imageHeight + YPosition + colliderDiameterY);
            }
        }

        //private void UpdateCircle()
        //{
        //    circle.SetCircleData(Center, Radius, 16);
        //    circle.Update();
        //}

        private void UpdateGameObjectPosition(GameTime gameTime)
        {
            base.gameObject.Transform.Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

    
    }
}
