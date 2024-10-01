using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.Components;
using System.Collections.Immutable;

namespace PandaGameLibrary.System;

public class RenderSystem
{
    public void Draw(SpriteBatch spriteBatch, ImmutableList<GameObject> gameObjects, GameTime gameTime)
	{
		GameObject[] sortedObjects = gameObjects.OrderBy((GameObject gameobject) => gameobject?.Transform?.Position.Y).ToArray();
		GameObject[] array = sortedObjects;
		foreach (GameObject gameObject in array)
		{
			if(gameObject != null)
			{
                gameObject.Draw(spriteBatch, gameTime);
            }
        }
	}

}
