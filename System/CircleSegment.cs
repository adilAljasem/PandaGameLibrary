using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PandaGameLibrary.System;

internal class CircleSegment
{
	public List<CircleSegment> circleSegments = new List<CircleSegment>();

	public Vector2 Start { get; set; }

	public Vector2 End { get; set; }
}
