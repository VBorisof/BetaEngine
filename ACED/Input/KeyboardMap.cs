using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;

namespace aced.Input;

public class KeyboardMap
{
    private readonly Dictionary<EditorEventType, HashSet<Keys>> _map = [];

    public KeyboardMap()
    {
        _map[EditorEventType.MoveLeft] = [Keys.Left];
        _map[EditorEventType.MoveRight] = [Keys.Right];
        _map[EditorEventType.MoveUp] = [Keys.Up];
        _map[EditorEventType.MoveDown] = [Keys.Down];
        _map[EditorEventType.ZoomIn] = [Keys.OemPlus];
        _map[EditorEventType.ZoomOut] = [Keys.OemMinus];

        _map[EditorEventType.Delete] = [Keys.Delete];
        _map[EditorEventType.Scale] = [Keys.S];
        _map[EditorEventType.Cancel] = [Keys.Escape];
    }

    public bool IsMatch(EditorEventType eventType, Keys? key)
    {
        if (key is null)
        {
            throw new InvalidOperationException("Cannot match null key.");
        }

        if (_map.TryGetValue(eventType, out var keys))
        {
            return keys.Contains(key.Value);
        }

        return false;
    }
}