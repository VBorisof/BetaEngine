using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Beta.Gui.Events;

public record GuiDragEventArgs
{
    public required Vector2 Position { get; init; }
    public required Vector2 DragVector { get; init; }
}
