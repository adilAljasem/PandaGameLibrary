using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PandaGameLibrary.System;

internal class CircleData
{
	private Texture2D pixel;

	private bool needsRecalculation = true;

	private List<CircleSegment> circleSegments = new List<CircleSegment>();

	public Vector2 Center { get; private set; }

	public float Radius { get; private set; }

	public int Segments { get; private set; }
    private Color Color { get; set; }

    public CircleData(Color color)
	{
		this.Color = color;
		pixel = new Texture2D(PandaCore.Instance.Game.GraphicsDevice, 1, 1);
		pixel.SetData(new Color[1] { Color });
	}

	public void SetCircleData(Vector2 center, float radius, int segments)
	{
		if (Center != center || Radius != radius || Segments != segments)
		{
			Center = center;
			Radius = radius;
			Segments = segments;
			needsRecalculation = true;
		}
	}

	public void Update()
	{
		if (needsRecalculation)
		{
			CalculateCircleSegments();
			needsRecalculation = false;
		}
	}

	private void CalculateCircleSegments()
	{
		circleSegments.Clear();
		float increment = (float)Math.PI * 2f / (float)Segments;
		float theta = 0f;
		for (int i = 0; i < Segments; i++)
		{
			Vector2 start = Center + Radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
			theta += increment;
			Vector2 end = Center + Radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
			circleSegments.Add(new CircleSegment
			{
				Start = start,
				End = end
			});
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		foreach (CircleSegment segment in circleSegments)
		{
			DrawLine(spriteBatch, segment.Start, segment.End);
		}
	}

	private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
	{
		Vector2 edge = end - start;
		float angle = (float)Math.Atan2(edge.Y, edge.X);
		spriteBatch.Draw(pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1), null, Color, angle, Vector2.Zero, SpriteEffects.None, 0f);
	}
}
