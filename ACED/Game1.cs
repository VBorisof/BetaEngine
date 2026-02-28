using Beta.Logging;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.DI;
using Beta.Input;
using Gtk;
using System;
using System.Threading;
using Beta.Gui;
using Beta.Common.Extensions;
using aced.Editors;
using aced.Input;

namespace aced;

public class Game1 : Game
{
    private Thread _gtkThread;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private readonly ILogger _logger;
    private readonly InputContextManager _inputContextManager;
    private readonly ITextManager _textManager;
    private readonly InputService _input;

    private EditorManager _editorManager;

    private const int MaxUpdateFailCount = 10;
    private int _updateFailCount;
    private const int MaxDrawFailCount = 10;
    private int _drawFailCount;

    public Game1()
    {   
        _graphics = new GraphicsDeviceManager(this);

        _textManager = new TextManagerModule().MakeTextManager();
        _logger = new ConsoleLogger(LogLevel.Debug);

        _inputContextManager = new InputContextManager();
        DependencyContainer.Instance.Add<InputContextManager>(_inputContextManager);

        _input = new InputService(_inputContextManager.GetOrCreateByName(nameof(Gui)));
        DependencyContainer.Instance.Add<InputService>(_input);

        DependencyContainer.Instance.Add<ITextManager>(_textManager);
        DependencyContainer.Instance.Add<ILogger>(_logger);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        var keybpoardMap = new KeyboardMap();
        DependencyContainer.Instance.Add<KeyboardMap>(keybpoardMap);

        _logger.Debug("Editor is starting...");
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.ApplyChanges();
        DependencyContainer.Instance.Add<GraphicsDeviceManager>(_graphics);
        DependencyContainer.Instance.Add<GameWindow>(Window);

        _gtkThread = new Thread(() =>
            {
                Application.Init();
                Application.Run();
            }
        );
        _gtkThread.Start();

        base.Initialize();

        Window.Title = "[3] (AC)tor (ED)itor";
    }

    protected override void LoadContent()
    {
        _textManager.Load(Content);

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Gui.Instance.Load(Content, _input, _inputContextManager, new GuiConfiguration
        {
            BaseLayerDepth = Constants.BaseLayerDepth,
            GuiFontBinding = new FontBinding(20, "dialogue", "dialogue"),
            LogLevel = LogLevel.Info,
            LayoutFile = "Layouts/layout.xml",
            StyleFile = "Layouts/style.css",
        });

        DependencyContainer.Instance.Add<ActorEditor>(new ActorEditor());
        DependencyContainer.Instance.Add<SceneEditor>(new SceneEditor());

        _editorManager = new EditorManager();
        _editorManager.OnExit += (_, _) => { Exit(); };
        DependencyContainer.Instance.Add<EditorManager>(_editorManager);

        Beta.GuiHandlerRegistries.GuiHandlerRegistryBootstrap.Init();

        _input.AddListener(_editorManager);
    }

    protected override void UnloadContent()
    {
        Application.Quit();
        _gtkThread.Join();

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            return;
        }

        var action = () =>
        {
            _editorManager.Update(gameTime);

            _input.Update(gameTime);

            Gui.Instance.Update(gameTime);

            base.Update(gameTime);

            _updateFailCount = 0;
        };

        WithRetry(action, _updateFailCount, MaxUpdateFailCount);
    }
    protected override void Draw(GameTime gameTime)
    {
        var action = () =>
        {
            GraphicsDevice.Clear(ColorEx.FromHexString("#060b1f"));
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );

            Gui.Instance.Draw(_spriteBatch);
            _editorManager.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);

            _drawFailCount = 0;
        };

        WithRetry(action, _updateFailCount, MaxUpdateFailCount);
    }

    private void WithRetry(System.Action action, int failCount, int maxFails)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            _logger.Error($"Exception: {e}");
            _logger.Error();
            _logger.Error();
            _logger.Error();
            ++failCount;
            if (failCount >= maxFails)
            {
                _logger.Error();
                _logger.Error();
                _logger.Error($"    Crashed after {failCount} tries!");
                _logger.Error();
                _logger.Error();
                throw;
            }
        }
    }

    private void OnHotReload(object sender, InputEventArgs e)
    {
        _logger.Debug("HOT RELOAD!");
        Gui.Instance.Reload();
    }
}