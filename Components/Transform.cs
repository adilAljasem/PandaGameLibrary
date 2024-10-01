using Microsoft.Xna.Framework;

namespace PandaGameLibrary.Components;

public class Transform
{
	public Vector2 Position { get; set; }

	public float Rotation { get; set; }

	public Vector2 Scale { get; set; }

	public Transform()
	{
		Position = Vector2.Zero;
		Rotation = 0f;
		Scale = Vector2.One;
	}

	public Transform(Vector2 position, float rotation, Vector2 scale)
	{
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}
}
