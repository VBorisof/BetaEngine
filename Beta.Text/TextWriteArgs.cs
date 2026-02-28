using Microsoft.Xna.Framework;

namespace Beta.Text;

public record TextWriteArgs
{
    public required FontBinding FontBinding { get; init; }
    public required Vector2 Position { get; set; }
    public required Color Color { get; init; }
    public Color OutlineColor { get; init; } = Color.Black;
    public TextAlignment TextAlignment { get; init; } = TextAlignment.Left;
    public float? LayerDepth { get; init; }

    public TextWriteArgs Copy()
    {
        return new TextWriteArgs
        {
            FontBinding = FontBinding,
            Position = Position,
            Color = Color,
            OutlineColor = OutlineColor,
            TextAlignment = TextAlignment,
            LayerDepth = LayerDepth,
        };
    }
}

