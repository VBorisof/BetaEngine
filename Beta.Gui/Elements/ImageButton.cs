using Beta.Gui.Events;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Beta.Gui.Elements;

public class ImageButton : GuiElement
{
    private readonly Box _box;
    private readonly Image _image;

    private readonly Label? _tooltip;

    public EventHandler OnLeftClick { get; } = (_, _) => { };


    private bool _wasHoverEnded = false;

    public ImageButton(Texture2D texture, GuiElementStyle style, string? tooltip, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _box = new Box(style with
        {
            RelativePosition = new Vector2(0, 0),
            LayerDepth = style.LayerDepth,
            Color = Color.Transparent,
        }, extraInputContexts);

        var padding = 4f;
        _image = new Image(texture, style with
        {
            RelativePosition = new Vector2(padding, padding),
            Size = style.Size - new SizeF(padding * 2, padding * 2),
            LayerDepth = style.LayerDepth + Constants.LayerDepthStep
        }, extraInputContexts);

        AddElement(_box);
        _box.AddElement(_image);

        if (tooltip is not null)
        {
            _tooltip = new Label(tooltip, style with
            {
                RelativePosition = Vector2.Zero,
                Hidden = true,
                LayerDepth = style.LayerDepth + Constants.LayerDepthMacroStep,
            }, extraInputContexts);
            AddElement(_tooltip);
        }

        SubscribeOnHover((_, args) =>
        {
            if (_tooltip is not null)
            {
                _tooltip.Style.Hidden = false;
                _tooltip.Style.RelativePosition =
                    args.Position - GetAbsolutePosition()
                    + new Vector2(-_tooltip.Style.Size.Width / 2, 50);
            }

            if (_wasHoverEnded)
            {
                Gui.Instance.PlaySound(GuiSoundType.Hover);
                _wasHoverEnded = false;
            }
        });
        SubscribeOnHoverEnd((_, __) =>
        {
            if (!_wasHoverEnded)
            {
                if (_tooltip is not null)
                {
                    _tooltip.Style.Hidden = true;
                }
                _wasHoverEnded = true;
            }
        });
        SubscribeOnLeftClick((_, __) =>
        {
            Gui.Instance.PlaySound(GuiSoundType.Click);
            GuiHandlerRegistry.InvokeHandlers(GuiEventType.LeftClick, ElemId);
            OnLeftClick(this, null!);
        });
        SubscribeOnRightClick((_, __) =>
        {
            GuiHandlerRegistry.InvokeHandlers(GuiEventType.RightClick, ElemId);
        });

        SubscribeOnDrag((_, _) =>
        {
            GuiHandlerRegistry.InvokeHandlers(GuiEventType.LeftPress, ElemId);
        });
    }

    public override string ToString()
    {
        return $"{nameof(ImageButton)} .{ElemClass} #{ElemId}";
    }
}