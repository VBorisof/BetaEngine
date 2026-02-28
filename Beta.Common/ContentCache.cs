using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Beta.Common;

public class ContentCache : IDisposable
{
    private readonly ContentManager _content;

    public string RootDirectory { get; }

    private readonly Dictionary<string, object> _cache = [];

    public ContentCache(ContentManager content)
    {
        _content = content;
        RootDirectory = _content.RootDirectory;
    }

    public T Get<T>(string path)
    {
        T result;
        if (!_cache.TryGetValue(path, out var value))
        {
            // Will actually throw an exception if doesn't exist,
            // so null check below is kinda pointless.
            var loaded = _content.Load<T>(path);
            if (loaded is null)
            {
                throw new ArgumentException($"Failed to load content {path}");
            }

            value = loaded;
            _cache[path] = value;
        }

        result = (T)value;
        return result;
    }

    public T? GetOrDefault<T>(string path)
    {
        T result;
        if (!_cache.TryGetValue(path, out var value))
        {
            try
            {
                var loaded = _content.Load<T>(path);

                value = loaded!;
                _cache[path] = value;
            }
            catch (ContentLoadException)
            {
                return default;
            }
        }

        result = (T)value;
        return result;
    }

    public void Dispose()
    {
        foreach (var obj in _cache)
        {
            if (obj.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
