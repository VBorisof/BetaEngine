using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Beta.Gui.Behaviors;
using Beta.Gui.Events;
using Beta.Gui.Styles;
using Beta.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class GuiElement : IInputEventListener
{
    private GuiElementBehavior? _behavior;

    public string ElemClass { get; set; } = "";
    public string ElemId { get; set; } = "";
    public string? ExtraInputContexts { get; set; }

    public GuiElementStyle Style { get; set; }

    public GuiElement? Parent { get; set; }

    public List<GuiElement> Children { get; } = [];

    private EventHandler<GuiMouseEventArgs> _onLeftClick = (_, _) => { };
    private EventHandler<GuiMouseEventArgs> _onRightClick = (_, _) => { };
    private EventHandler<GuiMouseEventArgs> _onLeftPress = (_, _) => { };
    private EventHandler<GuiScrollEventArgs> _onScroll = (_, _) => { };
    private EventHandler<GuiMouseEventArgs> _onHover = (_, _) => { };
    private EventHandler<GuiDragEventArgs> _onDrag = (_, _) => { };
    private EventHandler _onHoverEnd = (_, _) => { };
    private EventHandler<Keys> _onKeyHit = (_, _) => { };
    private bool _isHoverSubscribed;
    private bool _isLeftClickSubscribed;
    private bool _isRightClickSubscribed;
    private bool _isLeftPressSubscribed;
    private bool _isScrollSubscribed;
    private bool _isDragSubscribed;
    private bool _isKeyHitSubscribed;

    protected bool IsHovered { get; private set; }

    public GuiElement(GuiElementStyle style, string? extraInputContexts)
    {
        Style = style;
        ExtraInputContexts = extraInputContexts;
    }

    public virtual void AddElement(GuiElement element)
    {
        element.Parent = this;

        // If the element has extra input contexts, put them in.
        if (element.ExtraInputContexts is null)
        {
            element.ExtraInputContexts = ExtraInputContexts;
        }
        else
        {
            element.ExtraInputContexts += $" {ExtraInputContexts}";
        }
        Children.Add(element);

        // Only subscribe the element itself to updates,
        // because all the children are supposed to be added via this method,
        // and would already be subscribed.
        // TODO: Is this guaranteed?
        Gui.Instance.SubscribeElementToInputUpdates(element);
    }

    public void RemoveElement(GuiElement element)
    {
        element.Parent = null;
        Children.Remove(element);

        GuiSearch.ForEach(element, Gui.Instance.UnsubscribeElementFromInputUpdates);
    }

    public void RemoveChildren()
    {
        GuiSearch.ForEach(this, (descendant) =>
        {
            if (descendant == this)
            {
                return;
            }
            descendant.Parent?.Children.Remove(descendant);
            descendant.Parent = null;
            Gui.Instance.UnsubscribeElementFromInputUpdates(descendant);
        });
    }

    public void SetBehavior(GuiElementBehavior behavior)
    {
        _behavior?.OnDone();
        _behavior = behavior;
    }

    public void SetHidden(bool value)
    {
        GuiSearch.ForEach(this, descendant =>
        {
            descendant.Style.Hidden = value;
        });
    }

    public void SetDisabled(bool value)
    {
        GuiSearch.ForEach(this, descendant =>
        {
            descendant.Style.Disabled = value;
        });
    }

    public bool TryFindById<T>(string id, [NotNullWhen(true)] out T? result) where T : GuiElement
    {
        var elem = GuiSearch.FirstOrDefault(this, e => e.ElemId == id);

        result = null;
        if (elem is null || elem is not T castElem)
        {
            return false;
        }

        result = castElem;
        return true;
    }

    public bool TryFindByClass<T>(string cls, out T? result) where T : GuiElement
    {
        var elem = GuiSearch.FirstOrDefault(this, e => e.ElemClass == cls);
        result = null;
        if (elem is null || elem is not T castElem)
        {
            return false;
        }

        result = castElem;
        return true;
    }

    public T FindFirstById<T>(string id) where T : GuiElement
    {
        var elem = GuiSearch.FirstOrDefault(this, e => e.ElemId == id);
        if (elem is null || elem is not T t)
        {
            throw new GuiElementNotFoundException($"Gui element of type {typeof(T)} #{id} was not found.");
        }
        return t;
    }
    public T FindFirstByClass<T>(string cls) where T : GuiElement
    {
        var elem = GuiSearch.FirstOrDefault(this, e => e.ElemClass == cls);
        if (elem is null || elem is not T t)
        {
            throw new GuiElementNotFoundException($"Gui element of type {typeof(T)} .{cls} was not found.");
        }
        return t;
    }
    public IEnumerable<T> FindAllByClass<T>(string cls) where T : GuiElement
    {
        return GuiSearch
            .Where(this, e => e is T && e.ElemClass == cls)
            .Select(e => (T)e);
    }

    public virtual void Update(GameTime gameTime)
    {
        if (Style.Hidden)
        {
            return;
        }

        _behavior?.Update(gameTime);
        for (var i = Children.Count - 1; i >= 0; --i)
        {
            Children[i].Update(gameTime);
        }
    }
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (Style.Hidden)
        {
            return;
        }

        _behavior?.Draw(spriteBatch);
        for (var i = Children.Count - 1; i >= 0; --i)
        {
            Children[i].Draw(spriteBatch);
        }
    }

    public Vector2 GetAbsolutePosition()
    {
        if (Parent != null)
        {
            return Style.RelativePosition + Parent.GetAbsolutePosition();
        }
        return Style.RelativePosition;
    }

    public void SubscribeOnHover(EventHandler<GuiMouseEventArgs> callback)
    {
        _onHover += callback;
        _isHoverSubscribed = true;
    }
    public void SubscribeOnHoverEnd(EventHandler callback)
    {
        _onHoverEnd += callback;
    }

    public void SubscribeOnLeftClick(EventHandler<GuiMouseEventArgs> callback)
    {
        _onLeftClick += callback;
        _isLeftClickSubscribed = true;
    }
    public void SubscribeOnRightClick(EventHandler<GuiMouseEventArgs> callback)
    {
        _onRightClick += callback;
        _isRightClickSubscribed = true;
    }
    public void SubscribeOnLeftPress(EventHandler<GuiMouseEventArgs> callback)
    {
        _onLeftPress += callback;
        _isLeftPressSubscribed = true;
    }

    public void SubscribeOnScroll(EventHandler<GuiScrollEventArgs> callback)
    {
        _onScroll += callback;
        _isScrollSubscribed = true;
    }

    public void SubscribeOnDrag(EventHandler<GuiDragEventArgs> callback)
    {
        _onDrag += callback;
        _isDragSubscribed = true;
    }

    public void SubscribeOnKeyHit(EventHandler<Keys> callback)
    {
        _onKeyHit += callback;
        _isKeyHitSubscribed = true;
    }
    public void UnsubscribeOnKeyHit(EventHandler<Keys> callback)
    {
        if (_isKeyHitSubscribed)
        {
#pragma warning disable CS8601 // Rely on _isKeyHitSubscribed
            _onKeyHit -= callback;
#pragma warning restore CS8601
            _isKeyHitSubscribed = false;
        }
    }

    public InputEventConsumeResult OnMoveCursor(Vector2 pos, bool shouldTriggerHover)
    {
        if (!_isHoverSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        var isHovered = rect.Contains(pos);
        if (!isHovered)
        {
            _onHoverEnd(this, EventArgs.Empty);
            IsHovered = false;
        }
        else if (shouldTriggerHover)
        {
            _onHover(this, new GuiMouseEventArgs
            {
                Position = pos
            });
            IsHovered = true;
            //return new(markEvent: true);
        }

        return new();
    }

    public InputEventConsumeResult OnCursorMainAction(Vector2 pos)
    {
        if (!_isLeftClickSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        if (rect.Contains(pos))
        {
            _onLeftClick(this, new GuiMouseEventArgs
            {
                Position = pos
            });
            return new(swallowEvent: true);
        }
        return new();
    }

    public InputEventConsumeResult OnCursorSecondaryAction(Vector2 pos)
    {
        if (!_isRightClickSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        if (rect.Contains(pos))
        {
            _onRightClick(this, new GuiMouseEventArgs
            {
                Position = pos
            });
            return new(swallowEvent: true);
        }
        return new();
    }

    public InputEventConsumeResult OnCursorMainActionPressed(Vector2 pos)
    {
        if (!_isLeftPressSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        if (rect.Contains(pos))
        {
            _onLeftPress(this, new GuiMouseEventArgs
            {
                Position = pos
            });
            return new(swallowEvent: true);
        }
        return new();
    }

    public InputEventConsumeResult OnDrag(Vector2 pos, Vector2 dragVector)
    {
        if (!_isDragSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        if (rect.Contains(pos))
        {
            _onDrag(this, new GuiDragEventArgs
            {
                Position = pos,
                DragVector = dragVector
            });
        }

        return new();
    }

    public InputEventConsumeResult OnScroll(int scrollWheelDiff, Vector2 pos)
    {
        if (!_isScrollSubscribed)
        {
            return new();
        }

        var rect = new RectangleF(GetAbsolutePosition(), Style.Size);
        if (rect.Contains(pos))
        {
            _onScroll(this, new GuiScrollEventArgs
            {
                ScrollWheelDiff = scrollWheelDiff
            });
        }

        return new();
    }

    public InputEventConsumeResult OnKeyHit(Keys e)
    {
        // Need to check if in focus?
        if (!_isKeyHitSubscribed)
        {
            return new();
        }
        _onKeyHit(this, e);
        return new(swallowEvent: true);
    }

    public HashSet<InputContext> GetInputContexts()
    {
        var contexts = Gui.Instance.GetInputContexts(ExtraInputContexts);
        var parent = Parent;
        while (parent is not null)
        {
            foreach (var ctx in parent.GetInputContexts())
            {
                contexts.Add(ctx);
            }
            parent = parent.Parent;
        }
        return contexts;
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        var isHidden = Style.Hidden || (Parent?.Style?.Hidden ?? false);
        if (isHidden)
        {
            return new();
        }

        if (args.EventType == InputEventType.MouseMoved)
        {
            return OnMoveCursor(args.GetCursorPosition(), shouldTriggerHover: args.IsMarked == false);
        }
        if (args.EventType == InputEventType.LMBClicked || args.EventType == InputEventType.Tapped)
        {
            return OnCursorMainAction(args.GetCursorPosition());
        }
        if (args.EventType == InputEventType.RMBClicked)
        {
            return OnCursorSecondaryAction(args.GetCursorPosition());
        }
        if (args.EventType == InputEventType.LMBPressed)
        {
            return OnCursorMainActionPressed(args.GetCursorPosition());
        }
        if (args.EventType == InputEventType.ScrollWheelScrolled)
        {
            return OnScroll(args.GetScrollWheelDiff(), args.GetCursorPosition());
        }
        if (args.EventType == InputEventType.Dragged)
        {
            return OnDrag(args.GetCursorPosition(), args.GetTouchDragVector());
        }
        if (args.EventType == InputEventType.KeyHit)
        {
            return OnKeyHit(args.GetHitOrPressedKey());
        }

        return new();
    }

    public override string ToString()
    {
        return $"{nameof(GuiElement)} .{ElemClass} #{ElemId}";
    }
}