using System.Reflection;

namespace Beta.Gui.Events;

public record GuiEventHandlerMapping
{
    public required string ElemId { get; init; }
    public required GuiEventType GuiEventType { get; init; }
    public required MethodInfo Method { get; init; }
}