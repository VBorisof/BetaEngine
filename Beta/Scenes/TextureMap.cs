using Beta.DI;
using Beta.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Scenes;

public class TextureMap
{
    public Texture2D? Texture { get; private set; }
    private Color[] _pixels = [];
    private readonly ContentCache _contentCache;
    private readonly TextureMapCache _textureMapCache;

    public TextureMap()
    {
        _contentCache = DependencyContainer.Instance.Get<ContentCache>();
        _textureMapCache = DependencyContainer.Instance.Get<TextureMapCache>();
    }

    public bool Load(string path)
    {
        Texture = _contentCache.GetOrDefault<Texture2D>(path);
        if (Texture is null)
        {
            return false;
        }
        _pixels = _textureMapCache.Get(Texture);
        return true;
    }

    public Color GetPixel(Vector2 position)
    {
        if (Texture is null)
        {
            return new Color(1f, 1f, 1f, 1f);
        }

        var index = (int)position.X + ((int)position.Y * Texture.Width);
        if (index < 0 || index > _pixels.Length - 1 || _pixels[index].A == 0)
        {
            return new Color(1f, 1f, 1f, 1f);
        }
        return _pixels[index];
    }
}