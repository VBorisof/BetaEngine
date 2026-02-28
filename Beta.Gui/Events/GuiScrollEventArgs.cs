namespace Beta.Gui.Events;

public record GuiScrollEventArgs
{
    public required int ScrollWheelDiff { get; init; }
}