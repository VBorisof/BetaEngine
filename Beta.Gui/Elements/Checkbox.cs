using Beta.Gui.Events;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class Checkbox : GuiElement
{
    public bool Value { get; set; }
    public Label Label { get; }
    public Box Box { get; }

    public Checkbox(string text, bool initialValue, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        Value = initialValue;
        float hMargin = style.Size.Width / 2;

        float vMargin = style.Size.Height / 2;
        if (Gui.Instance.MainFont != null)
        {
            vMargin -= Gui.Instance.MainFont.VerticalSize / 2;
        }

        Label = new Label(text, style with
        {
            RelativePosition = new Vector2(0, vMargin)
        }, extraInputContexts);

        Box = new Box(style with
        {
            Color = Color.Transparent,
            RelativePosition = new Vector2(Label.Style.Size.Width + hMargin, 0),
        }, extraInputContexts);
        Box.SubscribeOnLeftClick((_, __) =>
        {
            Gui.Instance.PlaySound(GuiSoundType.Click);
            Value = !Value;
            GuiHandlerRegistry.InvokeBoolHandlers(GuiEventType.CheckboxToggle, ElemId, Value);
        });

        AddElement(Label);
        AddElement(Box);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        if (Value)
        {
            const int padding = 8;
            const int thickness = 2;
            var boxPos = Box.GetAbsolutePosition();
            var joint = boxPos + new Vector2(Box.Style.Size.Width / 2, Box.Style.Size.Height - padding);
            spriteBatch.DrawLine(
                point1: boxPos + new Vector2(padding, padding),
                point2: joint,
                color: Style.Color,
                thickness: thickness,
                layerDepth: Box.Style.LayerDepth + (Constants.LayerDepthStep * 2)
            );
            spriteBatch.DrawLine(
                point1: joint,
                point2: boxPos + new Vector2(Box.Style.Size.Width, -padding),
                color: Style.Color,
                thickness: thickness,
                layerDepth: Box.Style.LayerDepth + (Constants.LayerDepthStep * 2)
            );

        }
    }

    public override string ToString()
    {
        return $"{nameof(Checkbox)} .{ElemClass} #{ElemId}";
    }
}