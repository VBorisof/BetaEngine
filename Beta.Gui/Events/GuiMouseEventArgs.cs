using Microsoft.Xna.Framework;

namespace Beta.Gui.Events;

public record GuiMouseEventArgs
{
    public required Vector2 Position { get; init; }
}
