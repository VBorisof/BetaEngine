using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Linq;

namespace Beta.Input;

public record InputEventArgs
{
    public InputEventType EventType { get; }
    public MouseState? MState { get; set;  }
    public int? ScrollWheelDiff { get; set;  }
    public KeyboardState? KState { get; }
    public GamePadState? GpState { get; }
    public TouchCollection? Touches { get; set; }
    public Vector2? TouchDragVector { get; set; }
    public Keys? HitOrPressedKey { get; set; }
    public bool IsMarked { get; set; }

    public InputEventArgs(InputEventType eventType)
    {
        EventType = eventType;
    }

    public InputEventArgs(InputEventType eventType, MouseState mouseState)
    {
        EventType = eventType;
        MState = mouseState;
    }

    public InputEventArgs(InputEventType eventType, MouseState mouseState, int scrollWheelDiff)
    {
        EventType = eventType;
        MState = mouseState;
        ScrollWheelDiff = scrollWheelDiff;
    }

    public InputEventArgs(InputEventType eventType, KeyboardState keyboardState)
    {
        EventType = eventType;
        KState = keyboardState;
    }

    public InputEventArgs(InputEventType eventType, GamePadState gpState)
    {
        EventType = eventType;
        GpState = gpState;
    }

    public InputEventArgs(InputEventType eventType, TouchCollection touches)
    {
        EventType = eventType;
        Touches = touches;
    }
    public InputEventArgs(InputEventType eventType, TouchCollection touches, Vector2 touchDragVector)
    {
        EventType = eventType;
        Touches = touches;
        TouchDragVector = touchDragVector;
    }
    public InputEventArgs(InputEventType eventType, Keys hitKey)
    {
        EventType = eventType;
        HitOrPressedKey = hitKey;
    }

    public static InputEventArgs Empty => new(InputEventType.None);

    public int GetSelectedNumber()
    {
        return EventType switch
        {
            InputEventType.KeyHit or InputEventType.KeyPressed => HitOrPressedKey switch
            {
                Keys.D0 => 0,
                Keys.D1 => 1,
                Keys.D2 => 2,
                Keys.D3 => 3,
                Keys.D4 => 4,
                Keys.D5 => 5,
                Keys.D6 => 6,
                Keys.D7 => 7,
                Keys.D8 => 8,
                Keys.D9 => 9,
                _ => throw new InvalidOperationException($"Not a number key: {HitOrPressedKey}."),
            },
            _ => throw new InvalidOperationException($"Invalid EventType: {EventType}."),
        };
    }

    public Vector2 GetCursorPosition()
    {
        switch(EventType)
        {
            case InputEventType.MouseMoved:
            case InputEventType.LMBClicked:
            case InputEventType.LMBPressed:
            case InputEventType.RMBClicked:
            case InputEventType.RMBPressed:
            case InputEventType.ScrollWheelScrolled:
                {
                    if (MState is null)
                    {
                        throw new InvalidOperationException("Invalid mouse state.");
                    }
                    return MState.Value.Position.ToVector2();
                }
            case InputEventType.Tapped:
            case InputEventType.Dragged:
                {
                    if (Touches is null)
                    {
                        throw new InvalidOperationException("Invalid touchpad state.");
                    }
                    return Touches.Value.First().Position;
                }
            case InputEventType.LThumbStickMoved:
                {
                    if (GpState is null)
                    {
                        throw new InvalidOperationException("Invalid game pad state.");
                    }

                    // TODO: Fix this.
                    return GpState.Value.ThumbSticks.Left.ToPoint().ToVector2();
                }
            default:
                throw new InvalidOperationException($"Invalid EventType: {EventType}");
        }
    }

    public int GetScrollWheelDiff()
    {
        if (EventType != InputEventType.ScrollWheelScrolled)
        {
            throw new InvalidOperationException($"Invalid EventType: {EventType}");
        }
        if (ScrollWheelDiff is null)
        {
            throw new InvalidOperationException("Scroll wheel difference was not set.");
        }

        return ScrollWheelDiff.Value;
    }

    public Vector2 GetTouchDragVector()
    {
        if (EventType != InputEventType.Dragged)
        {
            throw new InvalidOperationException($"Invalid EventType: {EventType}");
        }
        if (TouchDragVector is null)
        {
            throw new InvalidOperationException($"Touch Drag Vector was null.");
        }

        return TouchDragVector.Value;
    }

    public Keys GetHitOrPressedKey()
    {
        if (EventType != InputEventType.KeyHit && EventType != InputEventType.KeyPressed)
        {
            throw new InvalidOperationException($"Invalid EventType: {EventType}.");
        }
        if (HitOrPressedKey is null)
        {
            throw new InvalidOperationException("Invalid keyboard state.");
        }

        return HitOrPressedKey.Value;
    }
}
