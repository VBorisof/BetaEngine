using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Beta.AdditionalUi;

public class FadingAdditionalUiElement
{
    private FadingAdditionalUiElementState _state = FadingAdditionalUiElementState.None;
    private bool _wantStop;

    private readonly Texture2D _texture;
    private readonly Rectangle _destinationRectangle;
    private readonly bool _isRotating;
    private TimeSpan _elapsed;
    private readonly TimeSpan _showForAtLeast = TimeSpan.FromSeconds(1);

    private float _alpha;
    private float _rotation;

    public FadingAdditionalUiElement(
        Texture2D texture,
        Rectangle destinationRectangle,
        bool isRotating,
        TimeSpan showForAtLeast)
    {
        _texture = texture;
        _destinationRectangle = destinationRectangle;
        _isRotating = isRotating;
        _showForAtLeast = showForAtLeast;
    }

    public void Start()
    {
        _state = FadingAdditionalUiElementState.Starting;
        _elapsed = TimeSpan.Zero;
        _alpha = 0.0f;
        _wantStop = false;
    }

    public void Stop()
    {
        _wantStop = true;
    }

    public void StopImmediately()
    {
        _wantStop = true;
        _state = FadingAdditionalUiElementState.None;
    }

    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;
        var alphaStep = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
        var rotationStep = 0.001f * gameTime.ElapsedGameTime.Milliseconds;
        switch (_state)
        {
            case FadingAdditionalUiElementState.None:
                break;
            case FadingAdditionalUiElementState.Running:
                if (_isRotating)
                {
                    _rotation += rotationStep;
                }
                if (_wantStop)
                {
                    _state = FadingAdditionalUiElementState.Stopping;
                }
                break;
            case FadingAdditionalUiElementState.Starting:
                if (_isRotating)
                {
                    _rotation += rotationStep;
                }
                _alpha += alphaStep;
                if (_alpha >= 1.0f)
                {
                    _state = FadingAdditionalUiElementState.Running;
                }
                break;
            case FadingAdditionalUiElementState.Stopping:
                if (_elapsed < _showForAtLeast)
                {
                    break;
                }

                if (_isRotating)
                {
                    _rotation += rotationStep;
                }
                _alpha -= alphaStep;
                if (_alpha <= 0.0f)
                {
                    _state = FadingAdditionalUiElementState.None;
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown auto-save spinner state {_state}."
                );
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_state != FadingAdditionalUiElementState.None)
        {
            spriteBatch.Draw(
                _texture,
                sourceRectangle: new Rectangle(0, 0, _texture.Width, _texture.Height),
                destinationRectangle: _destinationRectangle,
                effects: SpriteEffects.None,
                rotation: _rotation,
                origin: new Vector2(_texture.Width / 2, _texture.Height / 2),
                color: new Color(1f, 1f, 1f) * _alpha,
                layerDepth: Constants.LayerDepthGui - Constants.LayerDepthStep
            );
        }
    }
}
