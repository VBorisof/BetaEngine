using Beta.DI;
using Beta.Input;
using System.Collections.Generic;
using System.Linq;

namespace Beta.InputMapping;

public class InputMapper
{
    private readonly KeyboardMap _keyboardMap;
    private readonly List<InputMap> _mappings;

    public InputMapper()
    {
        _keyboardMap = DependencyContainer.Instance.Get<KeyboardMap>();
        _mappings = GenerateMaps();
    }

    public bool IsMatch(InputEventArgs eventArgs, GameInputType inputType)
    {
        return _mappings.Any(m =>
            m.GameInputType == inputType
            && m.IsMatch(eventArgs)
        );
    }
    public bool IsMatch(InputEventArgs eventArgs, IEnumerable<GameInputType> inputTypes)
    {
        return _mappings.Any(m =>
            inputTypes.Contains(m.GameInputType)
            && m.IsMatch(eventArgs)
        );
    }
    private bool IsMatchingKeyHit(GameInputType gameInputType, InputEventArgs args)
    {
        return args.EventType == InputEventType.KeyHit
            && _keyboardMap.IsMatch(gameInputType, args.HitOrPressedKey);
    }

    private bool IsMatchingKeyPressed(GameInputType gameInputType, InputEventArgs args)
    {
        return args.EventType == InputEventType.KeyPressed
            && _keyboardMap.IsMatch(gameInputType, args.HitOrPressedKey);
    }

    private List<InputMap> GenerateMaps()
    {
        return [
            new InputMap(GameInputType.OverlayCancel, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.OverlayCancel, args);
                var mouseClick = args.EventType == InputEventType.LMBClicked;
                return keyHit || mouseClick;
            }),
            new InputMap(GameInputType.Cancel, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.Cancel, args);
                return keyHit;
            }),
            new InputMap(GameInputType.ToggleMainMenu, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.ToggleMainMenu, args);
                return keyHit;
            }),
            new InputMap(GameInputType.MainMenuConfirm, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.MainMenuConfirm, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DoWalk, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DoWalk, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DoLook, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DoLook, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DoTalk, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DoTalk, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DoInteract, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DoInteract, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DoPickup, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DoPickup, args);
                return keyHit;
            }),
            new InputMap(GameInputType.ToggleInventory, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.ToggleInventory, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DialogueOptionPrev, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DialogueOptionPrev, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DialogueOptionNext, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DialogueOptionNext, args);
                return keyHit;
            }),
            new InputMap(GameInputType.DialogueOptionSubmit, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.DialogueOptionSubmit, args);
                return keyHit;
            }),
            new InputMap(GameInputType.ToggleHelp, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.ToggleHelp, args);
                return keyHit;
            }),

            //
            // DEBUG
            new InputMap(GameInputType.ToggleDebug, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.ToggleDebug, args);
                return keyHit;
            }),
            new InputMap(GameInputType.LogMore, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.LogMore, args);
                return keyHit;
            }),
            new InputMap(GameInputType.LogLess, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.LogLess, args);
                return keyHit;
            }),
            new InputMap(GameInputType.ReloadTutorial, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.ReloadTutorial, args);
                return keyHit;
            }),
            new InputMap(GameInputType.NextTutorial, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.NextTutorial, args);
                return keyHit;
            }),
            new InputMap(GameInputType.PrevTutorial, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.PrevTutorial, args);
                return keyHit;
            }),
            //
            //

            new InputMap(GameInputType.MoveUp, args =>
            {
                var keyPressed = IsMatchingKeyPressed(GameInputType.MoveUp, args);
                return keyPressed;
            }),
            new InputMap(GameInputType.MoveDown, args =>
            {
                var keyPressed = IsMatchingKeyPressed(GameInputType.MoveDown, args);
                return keyPressed;
            }),
            new InputMap(GameInputType.MoveLeft, args =>
            {
                var keyPressed = IsMatchingKeyPressed(GameInputType.MoveLeft, args);
                return keyPressed;
            }),
            new InputMap(GameInputType.MoveRight, args =>
            {
                var keyPressed = IsMatchingKeyPressed(GameInputType.MoveRight, args);
                return keyPressed;
            }),
            new InputMap(GameInputType.ShowHotspots, args =>
            {
                var keyPressed = IsMatchingKeyPressed(GameInputType.ShowHotspots, args);
                return keyPressed;
            }),
            new InputMap(GameInputType.CursorPositionChanged, args =>
            {
                return args.EventType == InputEventType.MouseMoved;
            }),
            new InputMap(GameInputType.CursorMainAction, args =>
            {
                return args.EventType == InputEventType.LMBClicked;
            }),
            new InputMap(GameInputType.CursorSecondaryAction, args =>
            {
                return args.EventType == InputEventType.RMBClicked;
            }),
            new InputMap(GameInputType.CursorMainActionAtPosition, args =>
            {
                return args.EventType == InputEventType.Tapped;
            }),
            new InputMap(GameInputType.CursorDragged, args =>
            {
                return args.EventType == InputEventType.Dragged;
            }),

            new InputMap(GameInputType.NumberSelected, args =>
            {
                var keyHit = IsMatchingKeyHit(GameInputType.NumberSelected, args);
                return keyHit;
            }),
        ];
    }
}
