using System;
using System.Collections.Generic;
using System.Linq;
using Beta.Common;
using Beta.Gui.Behaviors;
using Beta.Gui.Elements;
using Beta.Gui.Layouts;
using Beta.Gui.Styles;
using Beta.Input;
using Beta.Logging;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Gui;

public enum GuiSoundType
{
    Hover,
    Slide,
    Click,
    Toast,
}

public class Gui : Singleton<Gui>
{
    private GuiElement? _popup;
    private GuiElement? _root;
    private InputService? _inputService;
    private InputContextManager? _inputContextManager;
    private GuiConfiguration? _config;

    private SoundEffectInstance? _sfx;
    private float _sfxVolume = 1f;
    internal FontBinding? MainFont { get; private set; }
    internal ITextManager? TextManager { get; private set; }
    internal ILogger? Logger { get; private set; }

    public StyleManager? StyleManager { get; private set; }
    public LayoutManager? LayoutManager { get; private set; }

    private GuiState _state = GuiState.Normal;

    private readonly List<Toast> _toasts = [];

    public void Load(
        ContentManager content,
        InputService inputService,
        InputContextManager inputContextManager,
        GuiConfiguration config)
    {
        _config = config;
        MainFont = _config.GuiFontBinding;
        var fontBindings = new List<FontBinding>
        {
            MainFont
        };
        TextManager = new TextManager(fontBindings, defaultLayerDepth: 0.9f);
        TextManager.Load(content);

        Logger = new DummyLogger();

        _inputContextManager = inputContextManager;
        _inputService = inputService;

        // Load the styles...
        StyleManager = new StyleManager();
        StyleManager.AppendFromFile(_config.StyleFile);

        var contentCache = new ContentCache(content);

        // Load the layout files...
        LayoutManager = new LayoutManager(contentCache);
        _root = LayoutManager.LoadFromFile(_config.LayoutFile, _config.BaseLayerDepth);

        _inputService.AddListener(_root);
    }

    public void SubscribeElementToInputUpdates(GuiElement element)
    {
        if (_inputService is null)
        {
            throw new InvalidOperationException(
                "GUI is not loaded correctly: No InputService.");
        }
        _inputService.AddListener(element);

    }

    public void UnsubscribeElementFromInputUpdates(GuiElement element)
    {
        if (_inputService is null)
        {
            throw new InvalidOperationException(
                "GUI is not loaded correctly: No InputService.");
        }
        _inputService.RemoveListener(element);
    }

    public HashSet<InputContext> GetInputContexts(string? extraInputContexts = null)
    {
        if (_inputContextManager is null)
        {
            throw new InvalidOperationException(
                "GUI is not loaded correctly: No InputContextManager.");
        }

        HashSet<InputContext> inputContexts = [_inputContextManager.GetOrCreateByName(nameof(Gui))];
        if (extraInputContexts is not null)
        {
            var contextNames = extraInputContexts.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var contextName in contextNames)
            {
                inputContexts.Add(_inputContextManager.GetOrCreateByName(contextName));
            }
        }
        return inputContexts;

    }

    public GuiElement AppendToRoot(string layoutFile, string styleFile)
    {
        if (_config is null)
        {
            throw new InvalidOperationException("Not properly loaded.");
        }
        var elem = LoadFromFiles(
            layoutFile,
            styleFile,
            _config.BaseLayerDepth - Constants.LayerDepthMacroStep
        );

        _root!.AddElement(elem);
        return elem;
    }

    public GuiElement LoadFromFiles(string layoutFile, string styleFile, float layerDepth)
    {
        if (StyleManager is null || LayoutManager is null || _root is null || _config is null)
        {
            throw new InvalidOperationException("Not properly loaded.");
        }

        StyleManager.AppendFromFile(styleFile);
        var elem = LayoutManager.ReadAppendixFromFile(layoutFile, layerDepth);

        return elem;
    }

    public void RemoveFromRoot(GuiElement element)
    {
        if (_root is null)
        {
            throw new InvalidOperationException("Root element does not exist.");
        }
        _root.RemoveElement(element);
    }

    public void Reload()
    {
        if (_config is null)
        {
            throw new InvalidOperationException("Config was not loaded.");
        }
        if (_root is null)
        {
            throw new InvalidOperationException("Root is null.");
        }

        foreach (var toast in _toasts)
        {
            _root.RemoveElement(toast);
        }
        _toasts.Clear();

        StyleManager!.Clear();
        StyleManager!.AppendFromFile(_config.StyleFile);

        _root = LayoutManager!.LoadFromFile(_config.LayoutFile, _config.BaseLayerDepth);
        _popup = null;
        _state = GuiState.Normal;
    }

    public OkCancelMessageBox OkCancelMessageBox(string text)
    {
        if (StyleManager is null || _root is null)
        {
            var message = "Cannot create a popup: GUI is not loaded properly.";
            Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        var style = StyleManager.GetStyle("ok-cancel-message-box", "", "");
        style.LayerDepth = _root.Style.LayerDepth + Constants.LayerDepthMacroStep;
        var messageBox = new OkCancelMessageBox(text, style, extraInputContexts: null);

        _root.AddElement(messageBox);
        messageBox.OnOk += (_, __) =>
        {
            _root.RemoveElement(messageBox);
            _popup = null;
            _state = GuiState.Normal;
        };
        messageBox.OnCancel += (_, __) =>
        {
            _root.RemoveElement(messageBox);
            _popup = null;
            _state = GuiState.Normal;
        };

        _state = GuiState.Popup;
        _popup = messageBox;
        return messageBox;
    }

    public Toast Toast(string title, string text, TimeSpan? duration)
    {
        // TODO: Styling is all hardcoded :c

        if (StyleManager is null || _root is null)
        {
            var message = "Cannot create toast: GUI is not loaded properly.";
            Logger?.Error(message);
            throw new InvalidOperationException(message);
        }

        const int slideOffset = 80;
        var style = StyleManager.GetStyle("toast", null, null);
        style = style with
        {
            RelativePosition = style.RelativePosition + new Vector2(0, slideOffset),
        };

        // TODO: Hack hack hack -_____-
        var extraInputContexts = "gamestateplaying gamestateinventory";
        var toast = new Toast(style, title, text, duration, extraInputContexts: extraInputContexts);
        _toasts.Add(toast);

        // 
        // Only show one toast at any time.
        // Show next toast upon the previous one disappearing.
        if (_toasts.Count == 1)
        {
            _root.AddElement(toast);
            toast.SetBehavior(new GuiElementSlideBehavior(
                toast,
                toast.GetAbsolutePosition() - new Vector2(0, slideOffset)
            ));

            PlaySound(GuiSoundType.Toast);
        }
        toast.Done += (_, _) =>
        {
            _toasts.Remove(toast);
            _root.RemoveElement(toast);
            var nextToast = _toasts.FirstOrDefault();
            if (nextToast is not null)
            {
                nextToast.SetBehavior(new GuiElementSlideBehavior(
                    nextToast,
                    nextToast.GetAbsolutePosition() - new Vector2(0, slideOffset)
                ));
                _root.AddElement(nextToast);
                PlaySound(GuiSoundType.Toast);
            }
        };

        return toast;
    }

    public bool TryFindById<T>(string id, out T? result) where T : GuiElement
    {
        result = null;
        return _root?.TryFindById(id, out result) ?? false;
    }
    public T FindOneById<T>(string id) where T : GuiElement
    {
        return _root?.FindFirstById<T>(id)
            ?? throw new InvalidOperationException("GUI not loaded properly.");
    }

    public void Update(GameTime gameTime)
    {
        _root?.Update(gameTime);
        _popup?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _root?.Draw(spriteBatch);
        _popup?.Draw(spriteBatch);
    }

    public void PlaySound(GuiSoundType soundType)
    {
        _sfx?.Stop(immediate: true);

        switch (soundType)
        {
            case GuiSoundType.Hover:
                {
                    if (_config?.HoverUiElemSound is not null)
                    {
                        _sfx = _config.HoverUiElemSound.CreateInstance();
                        _sfx.Volume = _sfxVolume;
                        _sfx.Play();
                    }
                    break;
                }
            case GuiSoundType.Slide:
            case GuiSoundType.Click:
                {
                    if (_config?.ClickUiElemSound is not null)
                    {
                        _sfx = _config.ClickUiElemSound.CreateInstance();
                        _sfx.Volume = _sfxVolume;
                        _sfx.Play();
                    }
                    break;
                }
            case GuiSoundType.Toast:
                {
                    if (_config?.NotifySound is not null)
                    {
                        _sfx = _config.NotifySound.CreateInstance();
                        _sfx.Volume = _sfxVolume;
                        _sfx.Play();
                    }
                    break;
                }
            default:
                break;
        }
    }

    public void SetSoundVolume(float volume)
    {
        _sfxVolume = volume;
    }
}