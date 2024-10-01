using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PandaGameLibrary.System;

internal class AnimationManagerComponent
{
	private Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

	private Animation currentAnimation;

	private int FrameWidth;

	private int FrameHeight;

	public bool IsPlaying { get; private set; }

	public AnimationManagerComponent()
	{
	}

	public void AddAnimation(string name, Animation animation)
	{
		animation.FrameWidth = FrameWidth;
		animation.FrameHeight = FrameHeight;
		animations[name] = animation;
		if (currentAnimation == null)
		{
			currentAnimation = animation;
		}
	}

	internal void SetFrameSizeAnimation(int width, int height)
	{
		FrameHeight = height;
		FrameWidth = width;
	}

	public void PlayAnimation(string name)
	{
		if (animations.TryGetValue(name, out Animation animation))
		{
			currentAnimation = animation;
			IsPlaying = true;
		}
	}

	public Animation GetAnimation(string name)
	{
		if(animations.Count > 0) return animations[name];
		return null;
    }

    public Rectangle GetCurrentAnimationFirstFrame()
	{
		if (currentAnimation != null)
		{
			return currentAnimation.GetFirstFrame();
		}
		return Rectangle.Empty;
	}

	public void Pause()
	{
		IsPlaying = false;
	}

	public void Resume()
	{
		IsPlaying = true;
	}

	public void TogglePlayPause()
	{
		IsPlaying = !IsPlaying;
	}

	public void Update(GameTime gameTime)
	{
		if (IsPlaying)
		{
			currentAnimation?.Update(gameTime);
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 pos, Vector2 Origin,float scale, Color color,float rotation, float layer, SpriteEffects spriteEffects)
	{
		currentAnimation?.Draw(spriteBatch, pos, color,Origin, scale, rotation, layer, spriteEffects);
	}
}
