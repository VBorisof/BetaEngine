using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Beta.Input;

public class InputService
{
    public InputTransformer? InputTransformer { get; set; }
    public InputFilter? InputFilter { get; set; }

    public InputContext CurrentContext { get; set; }
    private readonly Dictionary<InputContext, List<IInputEventListener>> _listeners = [];
    private readonly List<ModifyInputListenersArgs> _toRemove = [];
    private readonly List<ModifyInputListenersArgs> _toAdd = [];

    public MouseState MState { get; private set; }
    private MouseState _oldMState;

    public KeyboardState KState { get; private set; }
    private KeyboardState _oldKState;

    public GamePadState GpState { get; private set; }
    private GamePadState _oldGpState;
    
    public TouchCollection Touches { get; private set; }
    private TouchCollection _oldTouches;

    public InputService(InputContext context)
    {
        CurrentContext = context;
    }

    public void AddListener(IInputEventListener listener)
    {
        if (_isDispatchingEvent)
        {
            _toAdd.Add(new ModifyInputListenersArgs
            {
                Listener = listener
            });
            return;
        }

        foreach (var context in listener.GetInputContexts())
        {
            // Create a mapping for this context if it doesn't exist.
            if (!_listeners.TryGetValue(context, out var listeners))
            {
                listeners = [];
                _listeners[context] = listeners;
            }
            else if (listeners.Contains(listener))
            {
                // TODO: Consider if add/remove errors should
                // throw exceptions. Sometimes fast conflicting actions
                // are more of a hassle to protect around than just ignoring
                // cases like this. Maybe just log this?
                /*
                throw new InvalidOperationException(
                    "Listener is already added.");
                */
                return;
            }

            listeners.Add(listener);
        }
    }

    public void RemoveListener(IInputEventListener listener)
    {
        if (_isDispatchingEvent)
        {
            _toRemove.Add(new ModifyInputListenersArgs
            {
                Listener = listener
            });
            return;
        }

        foreach (var context in listener.GetInputContexts())
        {
            var listeners = GetListeners(context);

            if (!listeners.Contains(listener))
            {
                /*
                throw new InvalidOperationException(
                    $"Listener not in context `{context.Name}`.");
                */
                return;
            }

            listeners.Remove(listener);
        }
    }

    public List<IInputEventListener> GetListeners(InputContext context)
    {
        if (!_listeners.TryGetValue(context, out var listeners))
        {
            throw new KeyNotFoundException(
                $"No context defined: `{context.Name}`");
        };
        return listeners;
    }

    private bool _isDispatchingEvent;

    private void DispatchEvent(InputEventArgs args)
    {
        if (!_listeners.TryGetValue(CurrentContext, out var listeners))
        {
            return;
        }
        if (InputTransformer is not null)
        {
            args = InputTransformer.Transform(args);
        }
        if (InputFilter is not null)
        {
            if (InputFilter.Filter.Invoke(args) == false)
            {
                return;
            }
        }

        _isDispatchingEvent = true;
        foreach (var listener in listeners)
        {
            var result = listener.OnInputEvent(args);
            if (result.SwallowEvent)
            {
                break;
            }
            if (result.MarkEvent)
            {
                args.IsMarked = true;
            }
        }
        _isDispatchingEvent = false;

        for (int i = _toAdd.Count - 1; i >= 0; --i)
        {
            AddListener(_toAdd[i].Listener);
            _toAdd.RemoveAt(i);
        }
        for (int i = _toRemove.Count - 1; i >= 0; --i)
        {
            RemoveListener(_toRemove[i].Listener);
            _toRemove.RemoveAt(i);
        }
    }

    public void Update(GameTime gameTime)
    {
        if (CurrentContext is null)
        {
            throw new InvalidOperationException("No context defined.");
        }

        ProcessMouse();
        ProcessKeyboard();
        ProcessTouch();
        ProcessGamepad();
    }

    private void ProcessMouse()
    {
        MState = Mouse.GetState();
        
        if (Vector2.Distance(MState.Position.ToVector2(), _oldMState.Position.ToVector2()) > 1)
        {
            DispatchEvent(new InputEventArgs(InputEventType.MouseMoved, MState));
        }
        
        if (MState.LeftButton == ButtonState.Released && _oldMState.LeftButton == ButtonState.Pressed)
        {
            DispatchEvent(new InputEventArgs(InputEventType.LMBClicked, MState));
        }
        
        if (MState.RightButton == ButtonState.Released && _oldMState.RightButton == ButtonState.Pressed)
        {
            DispatchEvent(new InputEventArgs(InputEventType.RMBClicked, MState));
        }
        
        if (MState.LeftButton == ButtonState.Pressed)
        {
            DispatchEvent(new InputEventArgs(InputEventType.LMBPressed, MState));
        }

        var scrollWheelDiff = (_oldMState.ScrollWheelValue - MState.ScrollWheelValue);
        if (scrollWheelDiff != 0)
        {
            DispatchEvent(new InputEventArgs(InputEventType.ScrollWheelScrolled, MState, scrollWheelDiff));
            _scrollWheelValue = 0;
        }

        _oldMState = MState;
    }

    private int _scrollWheelValue = 0;

    private void ProcessTouch()
    {
        Touches = TouchPanel.GetState();
        
        if (Touches.Count == 0 && _oldTouches.Count > 0)
        {
            DispatchEvent(new InputEventArgs(InputEventType.Tapped, _oldTouches));
        }
        if (Touches.Count == 1 && _oldTouches.Count == 1)
        {
            var touchDragVector = Touches.First().Position - _oldTouches.First().Position;
            DispatchEvent(new InputEventArgs(InputEventType.Dragged, Touches, touchDragVector));
        }
        
        _oldTouches = Touches;
    }

    private void ProcessKeyboard()
    {
        KState = Keyboard.GetState();

        foreach (var pressedKey in KState.GetPressedKeys())
        {
            DispatchEvent(new InputEventArgs(InputEventType.KeyPressed, pressedKey));
        }

        var previouslyPressedKeys = _oldKState.GetPressedKeys();
        foreach (var hitKey in previouslyPressedKeys.Where(KState.IsKeyUp))
        {
            DispatchEvent(new InputEventArgs(InputEventType.KeyHit, hitKey));
        }

        _oldKState = KState;
    }

    private void ProcessGamepad()
    {
        GpState = GamePad.GetState(PlayerIndex.One);

        // TODO

        _oldGpState = GpState;
    }
}
