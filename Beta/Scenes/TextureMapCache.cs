using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Scenes;

public class TextureMapCache
{
    private readonly Dictionary<Texture2D, Color[]> _cache = [];

    public Color[] Get(Texture2D texture)
    {
        Color[] result;
        if (!_cache.TryGetValue(texture, out var value))
        {
            value = (new Color[texture.Width * texture.Height]);
            _cache[texture] = value;
            texture.GetData(_cache[texture]);
        }

        result = value;
        return result;
    }
}