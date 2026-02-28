using Beta.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using System;

namespace Beta.InputMiddlewares;

public static class InputMiddlewareGenerator
{
    public static InputTransformer GenerateCameraInputTransformer(OrthographicCamera camera)
    {
        return new InputTransformer
        {
            Transform = (input) =>
            {
                if (input.EventType == InputEventType.MouseMoved
                    || input.EventType == InputEventType.LMBClicked
                    || input.EventType == InputEventType.LMBPressed
                    || input.EventType == InputEventType.RMBClicked
                    || input.EventType == InputEventType.ScrollWheelScrolled
                    || input.EventType == InputEventType.RMBPressed)
                {
                    if (input.MState is null || camera is null)
                    {
                        throw new InvalidOperationException("No mouse information.");
                    }

                    var worldPos = camera
                        .ScreenToWorld(
                            input.MState.Value.X,
                            input.MState.Value.Y
                        );

                    input.MState = new MouseState(
                        (int)worldPos.X,
                        (int)worldPos.Y,
                        input.MState.Value.ScrollWheelValue,
                        input.MState.Value.LeftButton,
                        input.MState.Value.MiddleButton,
                        input.MState.Value.RightButton,
                        input.MState.Value.XButton1,
                        input.MState.Value.XButton2
                    );
                }
                if (input.EventType == InputEventType.Tapped
                    || input.EventType == InputEventType.Dragged)
                {
                    if (input.Touches is null || camera is null)
                    {
                        throw new InvalidOperationException("No touch information.");
                    }

                    var firstTouch = input.Touches.Value[0];
                    var worldPos = camera
                        .ScreenToWorld(
                            firstTouch.Position.X,
                            firstTouch.Position.Y
                        );

                    input.Touches = new TouchCollection([
                        new TouchLocation(firstTouch.Id, firstTouch.State, worldPos),
                    ]);
                }

                return input;
            }
        };
    }

    public static InputFilter GenerateCameraInputFilter(OrthographicCamera camera)
    {
        return new InputFilter
        {
            // NB: Assume world positions
            Filter = (input) =>
            {
                if (input.EventType == InputEventType.MouseMoved
                    || input.EventType == InputEventType.LMBClicked
                    || input.EventType == InputEventType.RMBPressed)
                {
                    if (input.MState is null || camera is null)
                    {
                        throw new InvalidOperationException("No mouse information.");
                    }

                    if (input.MState.Value.X < 0 || input.MState.Value.X > camera.BoundingRectangle.Right
                        || input.MState.Value.Y < 0 || input.MState.Value.Y > camera.BoundingRectangle.Bottom)
                    {
                        return false;
                    }
                    return true;
                }
                if (input.EventType == InputEventType.Tapped
                    || input.EventType == InputEventType.Dragged)
                {
                    if (input.Touches is null || camera is null)
                    {
                        throw new InvalidOperationException("No touch information.");
                    }

                    var firstTouch = input.Touches.Value[0];
                    if (firstTouch.Position.X < 0 || firstTouch.Position.X > camera.BoundingRectangle.Right
                        || firstTouch.Position.Y < 0 || firstTouch.Position.Y > camera.BoundingRectangle.Bottom)
                    {
                        return false;
                    }
                }

                return true;
            }
        };
    }
}