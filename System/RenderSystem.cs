using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaGameLibrary.Components;
using System.Collections.Immutable;

namespace PandaGameLibrary.System;

public class RenderSystem
{
    public void Draw(SpriteBatch spriteBatch, ImmutableList<GameObject> gameObjects, GameTime gameTime)
    {
        var renderableComponents = gameObjects
            .Where(go => go != null && go.IsEnable)
            .SelectMany(go => go.GetComponents<Render2D>())
            .Where(render => render != null && render.IsEnabled)
            .OrderByDescending(render => render.LayerDepth) // Sort by layer depth first (higher values first)
            .ThenBy(render => render.gameObject.Transform?.Position.Y ?? float.MaxValue) // Then sort by Y position
            .ToArray();

        foreach (var render in renderableComponents)
        {
            render.Draw(spriteBatch, gameTime);
        }
    }

}
