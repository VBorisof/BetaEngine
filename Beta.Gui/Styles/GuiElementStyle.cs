using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Beta.Gui.Styles;

public record GuiElementStyle
{
    // LayerDepth, the higher, the top-er | BETA default = 0.8f
    public float LayerDepth { get; set; }

    public required SizeF Size { get; set; }
    public required Color Color { get; set; }
    public required Vector2 RelativePosition { get; set; }
    public Color HoverColor { get; set; }
    public Color TextColor { get; set; }
    public Color HoverTextColor { get; set; }
    public Color BorderColor { get; set; } = Color.Transparent;

    public int ChildItemHeight { get; set; }

    public bool Hidden { get; set; }
    public bool Disabled { get; set; }
    public Color DisabledColor { get; set; }
}