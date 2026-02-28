using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Beta.Gui.Styles;

public record GuiElementStyleQueryObject
{
    public float? LayerDepth { get; set; }
    public SizeF? Size { get; set; }
    public Color? Color { get; set; }
    public Color? HoverColor { get; set; }
    public Vector2? Position { get; set; }
    public Color? TextColor { get; set; }
    public Color? HoverTextColor { get; set; }
    public Color? BorderColor { get; set; }
    public int? ChildItemHeight { get; set; }
    public bool? Hidden { get; set; }
    public bool? Disabled { get; set; }
    public Color? DisabledColor { get; set; }
}