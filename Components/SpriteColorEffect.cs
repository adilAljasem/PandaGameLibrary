using Microsoft.Xna.Framework;
using PandaGameLibrary.Components;

internal class SpriteColorEffect
{
    private Render2D _render2D;

    private Color _originalColor;

    private Color _effectColor;

    private float _effectDuration;

    private float _effectTimer;

    private bool _isEffectActive;

    public SpriteColorEffect(Render2D render2D)
    {
        _render2D = render2D;
        _originalColor = render2D.Color;
      
    }

    public void TriggerEffect(Color effectColor, float effectDuration)
    {
        _effectColor = effectColor;
        _effectDuration = effectDuration;

        if (!_isEffectActive)
        {
            _render2D.Color = _effectColor;
            _effectTimer = _effectDuration;
            _isEffectActive = true;
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_isEffectActive)
        {
            _effectTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_effectTimer <= 0f)
            {
                _render2D.Color = _originalColor;
                _isEffectActive = false;
            }
        }
    }
}
