using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beta.Services;

public class ResolutionChangedEventArgs : EventArgs
{
    public required int Width { get; init; }
    public required int Height { get; init; }
}

public class GraphicsService
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly BoxingViewportAdapter _vpAdapter;
    private readonly List<DisplayMode> _supportedModes;
    private readonly IToastService _toastService;

    public event EventHandler<ResolutionChangedEventArgs> ResolutionChanged = (_, _) => { };
    public event EventHandler<bool> FullscreenChanged = (_, _) => { };

    public GraphicsService()
    {
        _graphics = DependencyContainer.Instance.Get<GraphicsDeviceManager>();
        _vpAdapter = DependencyContainer.Instance.Get<BoxingViewportAdapter>();
        _supportedModes = GetSupportedDisplayModes();
        _toastService = DependencyContainer.Instance.Get<IToastService>();
    }

    public Viewport GetViewport()
    {
        return _graphics.GraphicsDevice.Viewport;
    }
    public DisplayMode GetCurrentDisplayMode()
    {
        return GetDisplayMode(
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight
        ) ?? _graphics.GraphicsDevice.Adapter.CurrentDisplayMode;
    }
    public DisplayMode? GetDisplayMode(int width, int height)
    {
        return _supportedModes
            .FirstOrDefault(m => m.Width == width && m.Height == height);
    }

    public DisplayMode GetNextDisplayMode(DisplayMode? currentMode)
    {
        var mode = _supportedModes.Find(m => m == currentMode);
        if (mode is null)
        {
            return _supportedModes.First();
        }
        var currentModeIdx = _supportedModes.IndexOf(mode);

        if (currentModeIdx < _supportedModes.Count - 1)
        {
            return _supportedModes[currentModeIdx + 1];
        }
        return _supportedModes.First();
    }

    public DisplayMode GetPreviousDisplayMode(DisplayMode? currentMode)
    {
        var mode = _supportedModes.Find(m => m == currentMode);
        if (mode is null)
        {
            return _supportedModes.First();
        }
        var currentModeIdx = _supportedModes.IndexOf(mode);

        if (currentModeIdx > 0)
        {
            return _supportedModes[currentModeIdx - 1];
        }
        return _supportedModes.Last();
    }

    public bool TrySetFullscreen(bool value)
    {
        if (value)
        {
            var isSupported = _supportedModes.Any(
                d => d == _graphics.GraphicsDevice.Adapter.CurrentDisplayMode
            );

            if (!isSupported)
            {
                _toastService.Notify(5, "Target resolution is not supported.");
                return false;
            }
        }
        FullscreenChanged.Invoke(this, value);
        _graphics.IsFullScreen = value;
        _graphics.ApplyChanges();

        return true;
    }

    public bool TrySetDisplayMode(DisplayMode mode)
    {
        var isSupported = _supportedModes.Any(
            d => d == mode
        );

        if (!isSupported)
        {
            _toastService.Notify(5, "Target resolution is not supported.");
            return false;
        }

        _graphics.PreferredBackBufferWidth = mode.Width;
        _graphics.PreferredBackBufferHeight = mode.Height;
        _graphics.ApplyChanges();
        _vpAdapter.Reset();

        ResolutionChanged.Invoke(this, new ResolutionChangedEventArgs
        {
            Width = mode.Width,
            Height = mode.Height
        });

        return true;
    }

    private List<DisplayMode> GetSupportedDisplayModes()
    {
        return _graphics.GraphicsDevice.Adapter.SupportedDisplayModes
            .OrderBy(d => d.Width)
            .ToList();
    }
}