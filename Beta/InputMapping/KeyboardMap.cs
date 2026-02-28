using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Beta.InputMapping;

public class KeyboardMap
{
    private readonly Dictionary<GameInputType, HashSet<Keys>> _map = [];

    public KeyboardMap()
    {
        // TODO: Read from config

        //
        // Key down
        _map[GameInputType.MoveUp] = [Keys.Up];
        _map[GameInputType.MoveDown] = [Keys.Down];
        _map[GameInputType.MoveLeft] = [Keys.Left];
        _map[GameInputType.MoveRight] = [Keys.Right];
        _map[GameInputType.CameraLeft] = [Keys.Left];
        _map[GameInputType.CameraRight] = [Keys.Right];
        _map[GameInputType.ShowHotspots] = [Keys.Q];

        //
        // Key hit
        _map[GameInputType.Cancel] = [Keys.Escape];
        _map[GameInputType.OverlayCancel] = [Keys.Escape, Keys.F1];
        _map[GameInputType.ToggleMainMenu] = [Keys.Escape];
        _map[GameInputType.MainMenuConfirm] = [Keys.Enter];
        _map[GameInputType.DoWalk] = [Keys.X];
        _map[GameInputType.DoLook] = [Keys.W];
        _map[GameInputType.DoTalk] = [Keys.S];
        _map[GameInputType.DoInteract] = [Keys.A];
        _map[GameInputType.DoPickup] = [Keys.D];
        _map[GameInputType.ToggleInventory] = [Keys.E];
        _map[GameInputType.DialogueOptionPrev] = [Keys.Up];
        _map[GameInputType.DialogueOptionNext] = [Keys.Down];
        _map[GameInputType.DialogueOptionSubmit] = [Keys.Enter];

        _map[GameInputType.NumberSelected] = [
            Keys.D0,
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
        ];

        _map[GameInputType.ToggleHelp] = [Keys.F1];
        
        //
        // Debug/system
        _map[GameInputType.ToggleDebug] = [Keys.F3];
        _map[GameInputType.NextTutorial] = [Keys.L];
        _map[GameInputType.PrevTutorial] = [Keys.H];
        _map[GameInputType.ReloadTutorial] = [Keys.R];
        _map[GameInputType.LogMore] = [Keys.OemPlus];
        _map[GameInputType.LogLess] = [Keys.OemMinus];
    }

    public bool IsMatch(GameInputType inputType, Keys? key)
    {
        if (key is null)
        {
            throw new InvalidOperationException("Cannot match null key.");
        }

        if (_map.TryGetValue(inputType, out var keys))
        {
            return keys.Contains(key.Value);
        }

        return false;
    }
}
