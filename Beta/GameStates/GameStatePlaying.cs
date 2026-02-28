using BDSM.ExecutionContexts;
using Beta.Actors;
using Beta.AdditionalUi;
using Beta.BDSM;
using Beta.CommandManagement;
using Beta.Commands;
using Beta.Cursors;
using Beta.DI;
using Beta.Entities;
using Beta.Extensions;
using Beta.Input;
using Beta.Scenes;
using Beta.Services;
using Beta.Tutorials;
using Beta.Verbs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Beta.GameStates;

public class GameStatePlaying : GameState
{
    public override string Name => nameof(GameStatePlaying);

    private readonly Actor _player;
    private readonly InputService _inputService;
    private readonly OrthographicCamera _camera;
    private readonly EntityManager _entityManager;
    private readonly BDSMAdapter _bdsmAdapter;
    private readonly CommandManager _commandManager; 
    private readonly SceneManager _sceneManager;
    private readonly AdditionalUiManager _additionalUiManager;
    
    private readonly VerbManager _verbManager;
    private readonly VerbPickerMenu _verbMenu;

    private readonly GamePlayingDrawComponent _drawComponent = new();
    private readonly GamePlayingInteractionComponent _interactionComponent = new();

    private readonly TutorialService _tutorialService;

    public bool IsShowHotspots { get; set; }
    private CursorHoverSubject _hoverSubject = CursorHoverSubject.None;

    private SceneExit? _exit;
    private SceneProp? _prop;
    private Entity? _entity;
    private Vector2 _move;
    private Vector2 _tooltipPos;

    public event EventHandler MainMenuToggle = (_, _) => { };

    public GameStatePlaying(GameStateManager manager) : base(manager)
    {
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _player = _entityManager.Player ?? throw new InvalidOperationException("No player assigned.");

        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();

        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _inputService = DependencyContainer.Instance.Get<InputService>();

        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _additionalUiManager = DependencyContainer.Instance.Get<AdditionalUiManager>();

        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();

        _verbManager = DependencyContainer.Instance.Get<VerbManager>();

        _verbMenu = new();
        _verbMenu.OnSelectLook += OnSelectLook;
        _verbMenu.OnSelectPickup += OnSelectPickup;
        _verbMenu.OnSelectInteract += OnSelectInteract;
        _verbMenu.OnSelectTalk += OnSelectTalk;
        _verbMenu.OnCancel += OnCloseVerbMenu;

        _tutorialService = DependencyContainer.Instance.Get<TutorialService>();
    }

    private void InvokeMainMenuToggle()
    {
        MainMenuToggle.Invoke(this, EventArgs.Empty);
    }

    public override void Update(GameTime gameTime)
    {
        if (!_verbMenu.IsOpen)
        {
            var mousePos = _inputService.MState.Position.ToVector2();
            if (mousePos.X >= _camera.BoundingRectangle.Right - 100)
            {
                _camera.MoveCameraRight(_sceneManager);
            }
            if (mousePos.X <= _camera.Position.X + 100)
            {
                _camera.MoveCameraLeft();
            }
        }

        _commandManager.Update(gameTime);
        _entityManager.Update(gameTime);
        _sceneManager.Update(gameTime);
        _bdsmAdapter.Update(gameTime);
        _additionalUiManager.Update(gameTime);
        Gui.Gui.Instance.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _drawComponent.Draw(
            spriteBatch,
            _tooltipPos,
            _move,
            _verbMenu,
            IsShowHotspots,
            _hoverSubject,
            _prop,
            _entity
        );
        Gui.Gui.Instance.Draw(spriteBatch);

        IsShowHotspots = false;
    }

    private void OnMoveCursor(Vector2 scenePos)
    {
        _tooltipPos = scenePos + new Vector2(0, -80);

        if (_verbMenu.IsOpen)
        {
            _verbMenu.OnMoveCursor(scenePos);
        }
        else
        {
            if (!_commandManager.IsSharedQueueBusy())
            {
                QueryAtPos(scenePos);
            }
            else
            {
                _entity = null;
                _prop = null;
                _exit = null;
                _hoverSubject = CursorHoverSubject.None;
            }
        }
    }

    private void OnCursorDragged(Vector2 scenePos)
    {
        if (_verbMenu.IsOpen)
        {
            return;
        }

        _tooltipPos = scenePos + new Vector2(0, -160);
        QueryAtPos(scenePos);
    }

    private void QueryAtPos(Vector2 scenePos)
    {
        _entity = null;
        _prop = null;
        _exit = null;

        var result = _interactionComponent.QueryAtScenePos(scenePos);
        _hoverSubject = result.HoverSubject;
        switch (_hoverSubject)
        {
            case CursorHoverSubject.Entity:
                if (result.Entity is not null)
                {
                    _entity = result.Entity;
                    _verbManager.SuggestVerb(Verb.Walk);
                }
                break;
            case CursorHoverSubject.Prop:
                if (result.Prop is not null)
                {
                    _prop = result.Prop;
                    _verbManager.SuggestVerb(Verb.Walk);
                }
                break;
            case CursorHoverSubject.Exit:
                if (result.Exit is not null)
                {
                    _exit = result.Exit;
                    _verbManager.SuggestExit();
                }
                break;
            case CursorHoverSubject.WalkableArea:
                if (result.Move is not null)
                {
                    _move = result.Move.Value;
                    _verbManager.SuggestVerb(Verb.Walk);
                }
                break;
            case CursorHoverSubject.None:
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown hover subject: {result.HoverSubject}"
                );
        }
    }

    private void OnCursorMainActionAtPosition(Vector2 pos)
    {
        _tooltipPos = new Vector2(-100, -100);
        if (_verbMenu.IsOpen)
        {
            if (_verbMenu.IsCursorTooFar(pos))
            {
                _verbMenu.Close();
                OnCloseVerbMenu(this, EventArgs.Empty);
                return;
            }
        }
        else
        {
            QueryAtPos(pos);
        }
        OnCursorMainAction(pos);
    }

    private void OnCursorMainAction(Vector2 pos)
    {
        if (_commandManager.IsSharedQueueBusy())
        {
            _commandManager.Skip(_player);
            return;
        }

        // If we have a verb menu, try to click on one
        // of the options. The menu will not close here
        // if we clicked too far away, assume it's dealt with
        // before we call this method, kinda.
        if (_verbMenu.IsOpen)
        {
            _tutorialService.DoIfAllowed(
                MakeTutorialStepAction(TutorialStepActionType.VerbMenuClick),
                () => _verbMenu.OnCursorMainAction(pos)
            );
            
            return;
        }

        switch (_hoverSubject)
        {
            case CursorHoverSubject.Entity:
            case CursorHoverSubject.Prop:
                MainActionEntityOrProp(pos);
                return;
            case CursorHoverSubject.Exit:
                _tutorialService.DoIfAllowed(
                    MakeTutorialStepAction(TutorialStepActionType.Exit),
                    () => MainActionExit()
                );
                return;
            case CursorHoverSubject.WalkableArea:
            case CursorHoverSubject.None:
            default:
                _tutorialService.DoIfAllowed(
                    MakeTutorialStepAction(TutorialStepActionType.WalkableAreaLeftClick),
                    () => MainActionWalk(pos)
                );
                return;
        }
    }

    private void MainActionEntityOrProp(Vector2 pos)
    {
        // If we want a particular verb for this entity/prop
        if (_verbManager.WasVerbExplicitlySelected)
        {
            // First get past all the running commands,
            // one per call to this method.
            if (_commandManager.IsBusy(_player))
            {
                _commandManager.Skip(_player);
                return;
            }
            
            // If we're free from commands, apply the requested verb.
            ApplyCurrentVerb();
            return;
        }
        else
        {
            // Skip one command here if it's there.
            // Feels intuitively right, we don't want to empty the whole queue,
            // but if it's just one command, like if we're walking for example,
            // you want the menu to immediately open, and the player to just stop.
            if (_commandManager.IsBusy(_player))
            {
                _commandManager.Skip(_player);
                return;
            }

            // Otherwise, we just want to open the verb menu
            // to interact with this entity/prop.
            _tutorialService.DoIfAllowed(
                MakeTutorialStepAction(
                    _entity is null
                    ? TutorialStepActionType.PropLeftClick
                    : TutorialStepActionType.EntityLeftClick),
                () =>
                {
                    OpenVerbMenu(pos);
                }
            );
        }
    }

    private void MainActionExit()
    {
        if (_commandManager.IsBusy(_player))
        {
            // Are we impatient and/or nervous and clicking on the same exit?
            // If yes, just skip everything else and dispatch
            // immediate exit for maximum satisfaction.
            var prevExitCommand = _commandManager.FirstOrDefault<ExitCommand>(_player);
            if (prevExitCommand is not null)
            {
                // TODO: Need to do something with regions though...
                if (_exit == prevExitCommand.Exit)
                {
                    _commandManager.Interrupt(interruptAsync: false);
                    _commandManager.DispatchCommands(ExecutionContext.Shared, prevExitCommand);
                    return;
                }
            }

            // Otherwise, just do a normal command skip.
            _commandManager.Skip(_player);
            if (_commandManager.IsBusy())
            {
                return;
            }
        }
        if (_exit is not null)
        {
            _verbManager.RequestExit(_exit);
        }
    }

    private void MainActionWalk(Vector2 pos)
    {
        _verbManager.RequestMove(pos);
    }


    private void OnShowHotspots()
    {
        IsShowHotspots = true;
    }

    private void ApplyCurrentVerb()
    {
        switch (_hoverSubject)
        {
            case CursorHoverSubject.Entity:
                if (_entity is not null)
                {
                    _tutorialService.DoIfAllowed(
                        MakeTutorialStepAction(TutorialStepAction.GetActionTypeFromVerb(_verbManager.SelectedVerb)),
                        () => _verbManager.RequestApplyCurrentVerb(_entity)
                    );
                }
                break;
            case CursorHoverSubject.Prop:
                if (_prop is not null)
                {
                    _tutorialService.DoIfAllowed(
                        MakeTutorialStepAction(TutorialStepAction.GetActionTypeFromVerb(_verbManager.SelectedVerb)),
                        () => _verbManager.RequestApplyCurrentVerb(_prop)
                    );
                }
                break;
            case CursorHoverSubject.Exit:
                break;
            case CursorHoverSubject.WalkableArea:
                break;
            case CursorHoverSubject.None:
                break;
            default:
                break;
        }
    }

    private void OnMainMenuOpen()
    {
        if (_verbMenu.IsOpen)
        {
            _tutorialService.DoIfAllowed(
                MakeTutorialStepAction(TutorialStepActionType.VerbMenuDismiss),
                () =>
                {
                    _verbMenu.Close();
                }
            );
        }
        else
        {
            _tutorialService.DoIfAllowed(
                MakeTutorialStepAction(TutorialStepActionType.MainMenuOpen),
                () =>
                {
                    InvokeMainMenuToggle();
                }
            );
        }
    }

    private void OnSelectInventory()
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.InventoryOpen),
            () =>
            {
                _verbMenu.Close();
                Manager.RequestStateInventory();
            }
        );
    }

    private void OnCloseVerbMenuOrOpenInventory()
    {
        if (_verbMenu.IsOpen)
        {
            _tutorialService.DoIfAllowed(
                MakeTutorialStepAction(TutorialStepActionType.VerbMenuDismiss),
                () =>
                {
                    _verbMenu.Close();
                }
            );
        }
        else
        {
            OnSelectInventory();
        }
    }

    private void OnToggleHelp()
    {
        _verbMenu.Close();
        Manager.RequestStateOverlay("help");
    }

    private TutorialStepAction MakeTutorialStepAction(TutorialStepActionType actionType)
    {
        return new TutorialStepAction
        {
            ActionType = actionType,
            EntityName = _entity?.DeclName,
            PropName = _prop?.DeclName,
            ExitName = _exit?.Destination
        };
    }

    private void OnSelectTalk(object? sender, EventArgs e)
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.Talk),
            () =>
            {
                _verbMenu.Close();
                _verbManager.OnSelectTalk();
                ApplyCurrentVerb();
            }
        );
    }

    private void OnSelectPickup(object? sender, EventArgs e)
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.Pickup),
            () =>
            {
                _verbMenu.Close();
                _verbManager.OnSelectPickup();
                ApplyCurrentVerb();
            }
        );
    }

    private void OnSelectInteract(object? sender, EventArgs e)
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.Interact),
            () =>
            {
                _verbMenu.Close();
                _verbManager.OnSelectInteract();
                ApplyCurrentVerb();
            }
        );
    }

    private void OnSelectLook(object? sender, EventArgs e)
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.Look),
            () =>
            {
                _verbMenu.Close();
                _verbManager.OnSelectLook();
                ApplyCurrentVerb();
            }
        );
    }

    private void OnCloseVerbMenu(object? sender, EventArgs e)
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.VerbMenuDismiss),
            () =>
            {
                _verbMenu.Close();
                _verbManager.ResetVerb();
            }
        );
    }

    private void OpenVerbMenu(Vector2 pos)
    {
        if (!_verbMenu.IsOpen)
        {
            _verbMenu.SetPosition(pos);
            _verbMenu.Open();
            _verbManager.ResetVerb();
        }
    }

    public override void Reset()
    {
        _hoverSubject = CursorHoverSubject.None;
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CameraLeft))
        {
            _camera.MoveCameraLeft();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CameraRight))
        {
            _camera.MoveCameraRight(_sceneManager);
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorPositionChanged))
        {
            OnMoveCursor(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorDragged))
        {
            OnCursorDragged(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorMainAction))
        {
            OnCursorMainAction(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorMainActionAtPosition))
        {
            OnCursorMainActionAtPosition(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.CursorSecondaryAction))
        {
            OnCloseVerbMenuOrOpenInventory();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.ToggleInventory))
        {
            OnSelectInventory();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.ShowHotspots))
        {
            OnShowHotspots();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.ToggleMainMenu))
        {
            OnMainMenuOpen();
        }
        if (InputMapper.IsMatch(args, InputMapping.GameInputType.ToggleHelp))
        {
            OnToggleHelp();
        }

        return new();
    }
}
