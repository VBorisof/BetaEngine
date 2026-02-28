using Beta.Common.Extensions;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class Box : GuiElement
{
    public Box(GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        spriteBatch.DrawRectangle(
            GetAbsolutePosition(),
            Style.Size,
            Style.BorderColor,
            thickness: 1.5f,
            layerDepth: Style.LayerDepth + Constants.LayerDepthStep
        );

        spriteBatch.FillRectangle(
            GetAbsolutePosition(),
            Style.Size,
            Style.Disabled ? Style.DisabledColor : Style.Color,
            Style.LayerDepth
        );
    }

    public override string ToString()
    {
        return $"{nameof(Box)} .{ElemClass} #{ElemId}";
    }
}