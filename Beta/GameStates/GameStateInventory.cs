using Beta.CommandManagement;
using Beta.DI;
using Beta.Entities;
using Beta.GameInventory;
using Beta.Input;
using Beta.InputMapping;
using Beta.Scenes;
using Beta.Services;
using Beta.Tutorials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Beta.GameStates;

public class GameStateInventory : GameState
{
    protected Inventory Inventory { get; }
    private readonly OrthographicCamera _camera; // Needed for proper window placement.
    private readonly SceneManager _sceneManager;
    private readonly CommandManager _commandManager;
    private readonly EntityManager _entityManager;
    private readonly TutorialService _tutorialService;

    public override string Name => nameof(GameStateInventory);

    public GameStateInventory(GameStateManager manager, Inventory inventory)
        : base(manager)
    {
        Inventory = inventory;
        Inventory.View.Reset();

        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();

        var invPos =
            _camera.Center
            - new Vector2(
                Inventory.View.Width / 2,
                Inventory.View.Height / 2
            );
        Inventory.View.SetPosition(invPos);

        _tutorialService = DependencyContainer.Instance.Get<TutorialService>();
    }

    public override void Update(GameTime gameTime)
    {
        _entityManager.Update(gameTime);
        _commandManager.Update(gameTime);
        _tutorialService.Update(gameTime);

        Gui.Gui.Instance.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Inventory.View.Draw(spriteBatch);
        _sceneManager.Draw(spriteBatch);
        _commandManager.Draw(spriteBatch);

        Gui.Gui.Instance.Draw(spriteBatch);
    }

    private void OnCursorDragged(Vector2 pos)
    {
        var clickResult = Inventory.View.OnClick(pos);

        switch (clickResult.Intent)
        {
            case InventoryCellClickIntent.None:
            case InventoryCellClickIntent.ExitInventory:
                break;
            case InventoryCellClickIntent.PickItem:
                OnPickItem(pos, clickResult);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown Inventory intent: {clickResult.Intent}");
        }
    }
        
    private InputEventConsumeResult OnCursorMainAction(Vector2 pos)
    {
        var clickResult = Inventory.View.OnClick(pos);

        switch (clickResult.Intent)
        {
            case InventoryCellClickIntent.None:
                break;
            case InventoryCellClickIntent.ExitInventory:
                OnInventoryClose();
                return new(swallowEvent: true);
            case InventoryCellClickIntent.PickItem:
                OnPickItem(pos, clickResult);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown Inventory intent: {clickResult.Intent}");
        }

        return new();
    }

    private void OnPickItem(Vector2 initItemPos, InventoryCellClickResult clickResult)
    {
        if (clickResult.Actor is null
            || clickResult.Width is null
            || clickResult.Height is null)
        {
            throw new InvalidOperationException(
                "Missing inventory click result parameters."
            );
        }

        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.InventoryPickItem, clickResult.Actor.DeclName),
            () =>
            {
                Manager.RequestStateItemDrag(
                    clickResult.Actor,
                    initItemPos,
                    clickResult.Width.Value,
                    clickResult.Height.Value);
            }
        );
    }

    private void OnInventoryClose()
    {
        _tutorialService.DoIfAllowed(
            MakeTutorialStepAction(TutorialStepActionType.InventoryDismiss, itemName: null),
            () => Manager.RequestStatePlaying()
        );
    }

    private TutorialStepAction MakeTutorialStepAction(TutorialStepActionType actionType, string? itemName)
    {
        return new TutorialStepAction
        {
            ActionType = actionType,
            EntityName = null,
            PropName = null,
            ExitName = null,
            ItemName = itemName
        };
    }

    private void OnMoveCursor(Vector2 scenePos)
    {
        Inventory.View.OnMoveCursor(scenePos);
    }

    public override InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (InputMapper.IsMatch(args, GameInputType.CursorMainAction))
        {
            return OnCursorMainAction(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorMainActionAtPosition))
        {
            return OnCursorMainAction(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorPositionChanged))
        {
            OnMoveCursor(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, GameInputType.CursorDragged))
        {
            OnCursorDragged(args.GetCursorPosition());
        }
        if (InputMapper.IsMatch(args, [
                GameInputType.ToggleInventory,
                GameInputType.Cancel,
                GameInputType.CursorSecondaryAction
            ])
        )
        {
            OnInventoryClose();
        }

        return new();
    }
}