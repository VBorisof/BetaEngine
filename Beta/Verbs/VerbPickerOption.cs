using System;
using Beta.Common;
using Beta.DI;
using Beta.Services.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Verbs;

public class VerbPickerOption
{
    private readonly ContentCache _contentCache;

    private Texture2D _texture { get; set; }
    private Texture2D _hoverTexture { get; set; }

    private Texture2D _currentTexture { get; set; }

    private Vector2 _position { get; set; }

    private readonly int _iconWidth;
    private readonly int _iconHeight;

    private Rectangle _iconRect = new(0, 0, 0, 0);

    public EventHandler OnClick { get; set; } = (_, _) => { };
    private readonly SoundService _soundService;

    public VerbPickerOption(string path, string hoveredPath)
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();

        _texture = _contentCache.Get<Texture2D>(path);
        _hoverTexture = _contentCache.Get<Texture2D>(hoveredPath);

        _currentTexture = _texture;
        _iconWidth = 96;
        _iconHeight = _iconWidth * _currentTexture.Height / _currentTexture.Width;
    }

    public void SetPosition(Vector2 position)
    {
        _position = position;
        _position -= new Vector2(_iconWidth / 2, _iconHeight / 2);

        _iconRect = new Rectangle(
            (int)_position.X, (int)_position.Y, _iconWidth, _iconHeight
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _currentTexture,
            sourceRectangle: new Rectangle(
                0, 0, _currentTexture.Width, _currentTexture.Height
            ),
            destinationRectangle: _iconRect,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: Constants.LayerDepthGui
        );

        // TODO: Do we need a tooltip here?
    }

    public void OnCursorMoved(Vector2 pos)
    {
        var isHover = _iconRect.Contains(pos);

        // Kinda hacky, but use the texture to see if we're already
        // hovering, for seeing if we should play the sound
        if (isHover && _currentTexture != _hoverTexture)
        {
            _soundService.PlaySound(GameSoundType.Hover);
        }

        _currentTexture = isHover
            ? _hoverTexture
            : _texture;
    }

    public void Reset()
    {
        _currentTexture = _texture;
    }

    public void OnLeftClick(Vector2 pos)
    {
        if (_iconRect.Contains(pos))
        {
            _soundService.PlaySound(GameSoundType.Click);
            OnClick(this, EventArgs.Empty);
        }
    }
}
