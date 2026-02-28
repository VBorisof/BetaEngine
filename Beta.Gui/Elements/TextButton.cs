using System;
using Microsoft.Xna.Framework;
using Beta.Gui.Events;
using MonoGame.Extended;
using Beta.Gui.Styles;

namespace Beta.Gui.Elements;

public class TextButton : GuiElement
{
    private Label _label { get; set; }
    private Box _box { get; set; }

    private bool _wasHoverEnded = false;

    public string Text { get; }

    public EventHandler OnLeftClick { get; set; } = (_, _) => { };

    public TextButton(string text, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        Text = text;
        _label = new Label(
            text,
            style with
            {
                LayerDepth = style.LayerDepth + Constants.LayerDepthStep
            }
        , extraInputContexts);
        _label.Style.RelativePosition = new Vector2(
            (Style.Size.Width / 2) - (_label.Style.Size.Width / 2),
            (Style.Size.Height / 2) - (_label.Style.Size.Height / 2)
        );

        var minSize = _label.Style.Size;
        var boxWidth = style.Size.Width < minSize.Width ? minSize.Width : style.Size.Width;
        var boxHeight = style.Size.Height < minSize.Height ? minSize.Height : style.Size.Height;

        _box = new Box(style with
        {
            RelativePosition = new Vector2(0, 0),
            Size = new SizeF(boxWidth, boxHeight)
        }, extraInputContexts);

        _box.AddElement(_label);
        AddElement(_box);

        SubscribeOnHover((_, __) =>
        {
            if (Style.Disabled)
            {
                return;
            }
            if (_wasHoverEnded)
            {
                _label.Style.TextColor = style.HoverTextColor;
                _box.Style.Color = style.HoverColor;

                Gui.Instance.PlaySound(GuiSoundType.Hover);
                
                _wasHoverEnded = false;
            }
        });
        SubscribeOnHoverEnd((_, __) =>
        {
            if (Style.Disabled)
            {
                return;
            }

            if (!_wasHoverEnded)
            {
                _label.Style.TextColor = style.TextColor;
                _box.Style.Color = style.Color;

                _wasHoverEnded = true;
            }
        });
        SubscribeOnLeftClick((_, __) =>
        {
            if (Style.Disabled)
            {
                return;
            }
            Gui.Instance.PlaySound(GuiSoundType.Click);
            GuiHandlerRegistry.InvokeHandlers(GuiEventType.LeftClick, ElemId);
            OnLeftClick(this, null!);
        });
        SubscribeOnRightClick((_, __) =>
        {
            if (Style.Disabled)
            {
                return;
            }
            GuiHandlerRegistry.InvokeHandlers(GuiEventType.RightClick, ElemId);
        });
    }

    public override string ToString()
    {
        return $"{nameof(TextButton)} .{ElemClass} #{ElemId}";
    }
};