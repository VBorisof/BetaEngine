using System.Text;
using Beta.Dialogues;
using Beta.DI;
using Beta.Entities;
using Beta.Input;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Beta.Actors;
using Beta.Cursors;
using Beta.AdditionalUi;
using Beta.Verbs;
using System;
using Beta.Fades;
using System.Globalization;
using System.Collections.Generic;
using Beta.InputMapping;
using Beta.Services;

namespace Beta.GameStates;

public class GameStateManager : IInputEventListener
{
    public GameState State { get; private set; }
    public FadeOverlay Fade { get; }

    private readonly InputService _input;
    private readonly InputContextManager _inputContextManager;
    private readonly InputMapper _inputMapper;
    private readonly ITextManager _textManager;
    private readonly OrthographicCamera _camera;
    private readonly Cursor _cursor;
    private int _fps = 420;
    private readonly EntityManager _entityManager;
    private readonly TutorialService _tutorialService;
    private readonly AdditionalUiManager _additionalUiManager;
    private readonly VerbManager _verbManager;

    public event EventHandler RequestShowMainMenuGui = (_, _) => { };
    public event EventHandler RequestShowStartMenuGui = (_, _) => { };
    public event EventHandler RequestShowTutorialGui = (_, _) => { };
    public event EventHandler RequestTutorialBannerRemove = (_, _) => { };
    public event EventHandler MainMenuToggle = (_, _) => { };

    public GameStateManager()
    {
        _verbManager = DependencyContainer.Instance.Get<VerbManager>();
        _additionalUiManager = DependencyContainer.Instance.Get<AdditionalUiManager>();
        _input = DependencyContainer.Instance.Get<InputService>();
        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();
        _inputMapper = DependencyContainer.Instance.Get<InputMapper>();
        _textManager = DependencyContainer.Instance.Get<ITextManager>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _cursor = DependencyContainer.Instance.Get<Cursor>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _tutorialService = DependencyContainer.Instance.Get<TutorialService>();

        SetupTutorialEvents();

        Fade = new FadeOverlay();

        State = SetState(() => new GameStateLoading(this));
    }

    private void SetupTutorialEvents()
    {
        _tutorialService.RequestGuiControl += (_, _) =>
        {
            RequestStateTutorialGui();
        };
        _tutorialService.RequestTutorialBannerRemove += (_, _) =>
        {
            RequestTutorialBannerRemove.Invoke(this, EventArgs.Empty);
        };
        _tutorialService.ReleaseGuiControl += (_, _) =>
        {
            RequestStatePlaying();
        };
        _tutorialService.TutorialEnded += (_, _) =>
        {
            RequestStatePlaying();
        };
    }

    private TState SetState<TState>(Func<TState> generatorFunc, InputContext? inputContext = null) where TState : GameState
    {
        if (State is not null)
        {
            _input.RemoveListener(State);
            _cursor.SetCursor(Verb.Walk);
            State.Reset();
        }

        var state = generatorFunc();

        _input.AddListener(state);
        _input.CurrentContext = inputContext ?? _inputContextManager.GetOrCreateByName(state.Name);

        State = state;
        return state;
    }

    public void RequestStatePlaying()
    {
        _additionalUiManager.StopAllImmediately();

        var state = new GameStatePlaying(this);
        state.MainMenuToggle += MainMenuToggle;
        SetState(() => state);

        if (_tutorialService.IsInTutorial)
        {
            RequestShowTutorialGui.Invoke(this, EventArgs.Empty);
            _tutorialService.BeginTutorialIfNotStarted();
        }
        else
        {
            RequestShowMainMenuGui.Invoke(this, EventArgs.Empty);
        }
    }
    public void RequestStateInventory()
    {
        if (_entityManager.Player is null)
        {
            throw new InvalidOperationException("No player defined.");
        }
        SetState(() => new GameStateInventory(this, _entityManager.Player.Inventory));
    }
    public void RequestStateItemDrag(Actor item, Vector2 initItemPos, int itemWidth, int itemHeight)
    {
        if (_entityManager.Player is null)
        {
            throw new InvalidOperationException("No player defined.");
        }
        SetState(() => new GameStateItemDrag(this, _entityManager.Player.Inventory, item, initItemPos, itemWidth, itemHeight));
    }
    public void RequestStateDialogue(Dialogue dialogue, int nodeIndex)
    {
        if (_entityManager.Player is null)
        {
            throw new InvalidOperationException("No player defined.");
        }
        SetState(() => new GameStateDialogue(this, _entityManager.Player, dialogue, nodeIndex));
    }
    public void RequestStateMainMenu()
    {
        var state = SetState(() => new GameStateMainMenu(this), inputContext: _inputContextManager.GetOrCreateByName(nameof(Gui.Gui)));
        state.MainMenuToggle += MainMenuToggle;
    }
    public void RequestStateStartMenu()
    {
        var state = SetState(() => new GameStateStartMenu(this), inputContext: _inputContextManager.GetOrCreateByName(nameof(Gui.Gui)));
        RequestShowStartMenuGui.Invoke(this, EventArgs.Empty);
    }
    public void RequestStateTutorialGui()
    {
        var state = SetState(() => new GameStateTutorialGui(this), inputContext: _inputContextManager.GetOrCreateByName(nameof(Gui.Gui)));
    }
    public void RequestStateCinematic()
    {
        var state = new GameStateCinematic(this);
        SetState(() => state);
    }
    public void RequestStateVideo()
    {
        var state = new GameStateVideo(this);
        SetState(() => state);
    }
    public void RequestStateOverlay(string name)
    {
        var state = new GameStateOverlay(this, name);
        SetState(() => state);
    }

    public void RequestFadeIn(double speed)
    {
        Fade.State = FadeState.FadeIn;
        Fade.Speed = (float)speed;
    }

    public void RequestFadeOut(double speed)
    {
        Fade.State = FadeState.FadeOut;
        Fade.Speed = (float)speed;
    }

    public void Update(GameTime gameTime)
    {
        _fps = (int)(1 / gameTime.GetElapsedSeconds());

        Fade.Update(gameTime);

        State.Update(gameTime);

        _tutorialService.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Fade.Draw(spriteBatch);

        State.Draw(spriteBatch);

        var cursorPos = _camera.ScreenToWorld(_input.MState.X, _input.MState.Y);
        _cursor.Draw(spriteBatch, cursorPos);

        if (Settings.IsDebug)
        {
            DrawDebugInfo(spriteBatch);
        }

        _tutorialService.Draw(spriteBatch);
    }

    private void DrawDebugInfo(SpriteBatch spriteBatch)
    {
        var sb = new StringBuilder();
        var scenePos = _camera.ScreenToWorld(_input.MState.Position.ToVector2());
        sb.Append(CultureInfo.InvariantCulture, $"FPS: {_fps}\n");
        sb.Append(CultureInfo.InvariantCulture, $"Current State: {State.Name}\n");
        sb.Append(CultureInfo.InvariantCulture, $"MousePos: {_input.MState.Position}\n");
        sb.Append(CultureInfo.InvariantCulture, $"ScenePos: {scenePos}\n");
        sb.Append(CultureInfo.InvariantCulture, $"Player pos: {_entityManager.Player?.Position}\n");
        sb.Append(CultureInfo.InvariantCulture, $"Camera pos: {_camera.Position}\n");
        sb.Append(CultureInfo.InvariantCulture, $"Camera world pos: {_camera.WorldToScreen(Vector2.Zero)}\n");

        _textManager.WriteLine(
            spriteBatch,
            sb.ToString(),
            new TextWriteArgs
            {
                FontBinding = TextManagerModule.Debug,
                Position = _camera.Position + new Vector2(10, 10),
                Color = Color.Green,
                LayerDepth = Constants.LayerDepthDebug,
            }
        );

        spriteBatch.DrawLine(
            _camera.Position.X, _camera.Position.Y,
            _camera.Position.X, _camera.BoundingRectangle.Bottom,
            Color.Green,
            3,
            layerDepth: Constants.LayerDepthDebug
        );
        spriteBatch.DrawLine(
            _camera.BoundingRectangle.Right, _camera.Position.Y,
            _camera.BoundingRectangle.Right, _camera.BoundingRectangle.Height,
            Color.Green,
            3,
            layerDepth: Constants.LayerDepthDebug
        );
    }

    public void ResetState()
    {
        State.Reset();
        _verbManager.ResetVerb();
    }

    public HashSet<InputContext> GetInputContexts()
    {
        // TODO: Ehem..
        return [
            _inputContextManager.GetOrCreateByName(nameof(GameStateManager)),
            _inputContextManager.GetOrCreateByName(nameof(Gui.Gui)),

            _inputContextManager.GetOrCreateByName(nameof(GameState)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateCinematic)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateDialogue)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateInventory)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateItemDrag)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateLoading)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateMainMenu)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateOverlay)),
            _inputContextManager.GetOrCreateByName(nameof(GameStatePlaying)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateStartMenu)),
            _inputContextManager.GetOrCreateByName(nameof(GameStateVideo)),
        ];
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        // DEV
        /*
        if (_inputMapper.IsMatch(args, GameInputType.ToggleDebug))
        {
            Settings.IsDebug = !Settings.IsDebug;
        }
        if (_inputMapper.IsMatch(args, GameInputType.NextTutorial))
        {
            _tutorialService.GoToNextStep();
        }
        if (_inputMapper.IsMatch(args, GameInputType.PrevTutorial))
        {
            _tutorialService.GoToPrevStep();
        }
        if (_inputMapper.IsMatch(args, GameInputType.ReloadTutorial))
        {
            _tutorialService.LoadTutorial();
        }
        */

        return new();
    }
}