using System;
using Beta.Actors;
using Beta.CommandManagement;
using Beta.DI;
using Beta.Entities;
using Beta.GameInventory;
using Beta.Input;
using Beta.InputMapping;
using Beta.Scenes;
using Beta.Services;
using Beta.Services.Sounds;
using Beta.Tutorials;
using Beta.Verbs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.GameStates;

public class GameStateItemDrag : GameState
{
    protected readonly CommandManager _commandManager;
    protected readonly Actor _player;
    private readonly TutorialService _tutorialService;
    protected bool _isInInventory;
    protected readonly EntityManager _entityManager;
    protected readonly Scene _scene;
    protected readonly VerbManager _verbManager;
    protected readonly Actor _item;
    protected Vector2 _itemPos;
    
    private readonly int _width;
    private readonly int _height;

    protected Inventory Inventory { get; }
    private readonly SceneManager _sceneManager;
    private readonly SoundService _soundService;

    public override string Name => nameof(GameStateItemDrag);

    public GameStateItemDrag(GameStateManager manager, Inventory inventory, Actor item, Vector2 initItemPos, int itemWidth, int itemHeight) : base(manager)
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();
        _verbManager = DependencyContainer.Instance.Get<VerbManager>();
        _item = item;
        _itemPos = initItemPos;
        _width = itemWidth;
        _height = itemHeight;
        _isInInventory = true;
        Inventory = inventory;
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();

        _scene = _sceneManager.CurrentScene ?? throw new InvalidOperationException("No scene.");
        _player = _entityManager.Player ?? throw new InvalidOperationException("No player defined.");

        _tutorialService = DependencyContainer.Instance.Get<TutorialService>();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_isInInventory)
        {
            Inventory.View.Draw(spriteBatch);
        }

        _item.Position = _itemPos;
        _item.LayerDepth = Constants.LayerDepthGui + (Constants.LayerDepthStep * 2);
        _item.DrawSizedInPlace(spriteBatch, _width, _height);

        _sceneManager.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);

        Gui.Gui.Instance.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        _item.CurrentAnimation.Update(gameTime);
        _entityManager.Update(gameTime);
        _commandManager.Update(gameTime);
        Gui.Gui.Instance.Update(gameTime);
    }

    private void OnCursorMainActionAt(Vector2 scenePos)
    {
        if (_isInInventory)
        {
            // Interact with some item?
            var actor = Inventory.View.GetAtScreenPosOrDefault(scenePos);

            if (actor is not null && _item != actor)
            {
                _tutorialService.DoIfAllowed(
                    MakeTutorialStepAction(TutorialStepActionType.InventoryCombineItem, entityName: actor.DeclName, propName: null),
                    () =>
                    {
                        _soundService.PlaySound(GameSoundType.ItemUse);
                        _verbManager.SelectUse();
                        _verbManager.RequestApplyCurrentVerb(actor, _item);
                    }
                );
            }
            Manager.RequestStatePlaying();
        }
        else
        {
            // Try find entity
            foreach (var entity in _entityManager.GetOnScene(_scene))
            {
                if (entity.Contains(scenePos))
                {
                    _tutorialService.DoIfAllowed(
                        MakeTutorialStepAction(TutorialStepActionType.EntityUse, entityName: entity.DeclName, propName: null),
                        () =>
                        {
                            _soundService.PlaySound(GameSoundType.ItemUse);
                            _verbManager.SelectUse();
                            _verbManager.RequestApplyCurrentVerb(entity, _item);
                        }
                    );
                    Manager.RequestStatePlaying();
                    return;
                }
            }

            // Try find prop
            foreach (var prop in _scene.Props)
            {
                if (prop.Contains(scenePos))
                {
                    _tutorialService.DoIfAllowed(
                        MakeTutorialStepAction(TutorialStepActionType.PropUse, entityName: null, propName: prop.DeclName),
                        () =>
                        {
                            _soundService.PlaySound(GameSoundType.ItemUse);
                            _verbManager.SelectUse();
                            _verbManager.RequestApplyCurrentVerb(prop, _item);
                        }
                    );
                    Manager.RequestStatePlaying();
                    return;
                }
            }

            Manager.RequestStatePlaying();
        }
    }

    private void OnMoveCursor(Vector2 scenePos)
    {
        _itemPos = scenePos;
        if (Inventory.View.IsOutsideInventory(_itemPos))
        {
            _isInInventory = false;
        }
    }

    private void OnToggleInventory()
    {
        if (_isInInventory)
        {
            _isInInventory = false;
        }
        else
        {
            Manager.RequestStatePlaying();
        }
    }

    private TutorialStepAction MakeTutorialStepAction(TutorialStepActionType actionType, string? entityName, string? propName)
    {
        return new TutorialStepAction
        {
            ActionType = actionType,
            EntityName = entityName,
            PropName = propName,
            ExitName = null,
            ItemName = _item.DeclName
        };
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, GameInputType.CursorMainAction))
        {
            OnCursorMainActionAt(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorMainActionAtPosition))
        {
            OnCursorMainActionAt(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorPositionChanged))
        {
            OnMoveCursor(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorDragged))
        {
            OnMoveCursor(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, [
                GameInputType.ToggleInventory,
                GameInputType.Cancel,
                GameInputType.CursorSecondaryAction
            ])
        )
        {
            OnToggleInventory();
        }

        return new();
    }
}