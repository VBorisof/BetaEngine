using BDSM.Runtime;
using Beta.AdditionalUi;
using Beta.BDSM;
using Beta.CommandManagement;
using Beta.Common;
using Beta.ContentTools;
using Beta.Cursors;
using Beta.DI;
using Beta.Entities;
using Beta.GameStates;
using Beta.Input;
using Beta.InputMapping;
using Beta.InputMiddlewares;
using Beta.Logging;
using Beta.Scenes;
using Beta.Services;
using Beta.Services.Sounds;
using Beta.Text;
using Beta.Tutorials;
using Beta.Verbs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.ViewportAdapters;
using System;

namespace ExampleGame;

public class Game1 : Game
{
    private ILogger _logger;
    private ITextManager _textManager;
    private BDSMAdapter _bdsmAdapter;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private EntityManager _entityManager;
    private SceneManager _sceneManager;
    private CommandManager _commandManager;
    private HistoryService _historyService;
    private ContentPathProvider _contentPathProvider;
    private MusicPlayerService _musicPlayerService;
    private Driver _driver;
    private OrthographicCamera _camera;
    private ContentCache _contentCache;
    private GameStateManager _gameStateManager;
    private VerbManager _verbManager;
    private AdditionalUiManager _additionalUiManager;
    private InputService _input;
    private InputContextManager _inputContextManager;
    private InputMapper _inputMapper;
    private Cursor _cursor;
    private TutorialService _tutorialService;
    private InputService _inputService;
    private TutorialProvider _tutorialProvider;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        SetupSystemServices();

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        IsMouseVisible = false;

        base.Initialize();

        var vpAdapter = new BoxingViewportAdapter(
            Window,
            _graphics.GraphicsDevice,
            800,
            600
        );
        DependencyContainer.Instance.Add<BoxingViewportAdapter>(vpAdapter);

        _camera = new OrthographicCamera(vpAdapter);
        DependencyContainer.Instance.Add<OrthographicCamera>(_camera);

        SetupInputServices();

        EngineSetup();

        GameSetup();

        Window.Title = "Example Beta Game";
        _logger.Info("Initialization complete.");
    }

    private void SetupSystemServices()
    {
        _logger = new ConsoleLogger(LogLevel.Debug);
        DependencyContainer.Instance.Add<ILogger>(_logger);

        var font = new FontBinding(20, "betaf", "betaf_outline");
        _textManager = new TextManager([font], 1.0f);
        DependencyContainer.Instance.Add<ITextManager>(_textManager);

        // DependencyContainer.Instance.Add<IToastService>(new ToastService());

        Content.RootDirectory = "Content";
        DependencyContainer.Instance.Add<ContentManager>(Content);

        _contentPathProvider = new ContentPathProvider();
        DependencyContainer.Instance.Add<IContentPathProvider>(_contentPathProvider);

        _contentCache = new ContentCache(Content);
        DependencyContainer.Instance.Add<ContentCache>(_contentCache);
    }

    private void EngineSetup()
    {
        _musicPlayerService = new MusicPlayerService();
        DependencyContainer.Instance.Add<MusicPlayerService>(_musicPlayerService);

        _entityManager = new EntityManager();
        DependencyContainer.Instance.Add<EntityManager>(_entityManager);

        _sceneManager = new SceneManager();
        DependencyContainer.Instance.Add<SceneManager>(_sceneManager);

        _commandManager = new CommandManager();
        DependencyContainer.Instance.Add<CommandManager>(_commandManager);

        _historyService = new HistoryService();
        DependencyContainer.Instance.Add<HistoryService>(_historyService);

        _contentPathProvider = new ContentPathProvider();
        DependencyContainer.Instance.Add<ContentPathProvider>(_contentPathProvider);

        _driver = new Driver(BDSM.Logging.BDSMLogLevel.Debug);
        DependencyContainer.Instance.Add<Driver>(_driver);

        _bdsmAdapter = new BDSMAdapter();
        DependencyContainer.Instance.Add<BDSMAdapter>(_bdsmAdapter);
    }

    private void GameSetup()
    {
        _cursor = new Cursor();
        DependencyContainer.Instance.Add<Cursor>(_cursor);

        _additionalUiManager = new AdditionalUiManager();
        DependencyContainer.Instance.Add<AdditionalUiManager>(_additionalUiManager);

        _tutorialProvider = new TutorialProvider();
        DependencyContainer.Instance.Add<TutorialProvider>(_tutorialProvider);

        _tutorialService = new TutorialService();
        DependencyContainer.Instance.Add<TutorialService>(_tutorialService);

        _verbManager = new VerbManager();
        DependencyContainer.Instance.Add<VerbManager>(_verbManager);

        _gameStateManager = new GameStateManager();
        DependencyContainer.Instance.Add<GameStateManager>(_gameStateManager);

        var launchScriptPath = "Content/scripts/launch.bs";
        if (_bdsmAdapter is null || !_bdsmAdapter.LaunchGame(launchScriptPath))
        {
            Exit();
        }
    }

    private void SetupInputServices()
    {
        if (_camera is null)
        {
            throw new InvalidOperationException("Camera is not initialized.");
        }

        _inputContextManager = new InputContextManager();
        DependencyContainer.Instance.Add<InputContextManager>(_inputContextManager);

        _inputService = new InputService(_inputContextManager.GetOrCreateByName(nameof(GameStateManager)))
        {
            InputTransformer = InputMiddlewareGenerator.GenerateCameraInputTransformer(_camera)
        };
        DependencyContainer.Instance.Add<InputService>(_inputService);

        var keyboardMap = new KeyboardMap();
        DependencyContainer.Instance.Add<KeyboardMap>(keyboardMap);

        var inputMapper = new InputMapper();
        DependencyContainer.Instance.Add<InputMapper>(inputMapper);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _textManager.Load(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            _musicPlayerService.Pause();
            return;
        }
        _musicPlayerService.Resume();
        _musicPlayerService.Update(gameTime);

        _inputService.Update(gameTime);
        _gameStateManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        var viewMatrix = _camera.GetViewMatrix();

        _spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: viewMatrix
        );

        _gameStateManager.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
