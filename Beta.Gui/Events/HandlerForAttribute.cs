using System;

namespace Beta.Gui.Events;

[AttributeUsage(AttributeTargets.Method)]
public class HandlerForAttribute : Attribute
{
    public GuiEventType GuiEvent { get; }
    public string ElemId { get; }

    public HandlerForAttribute(GuiEventType guiEvent, string elemId)
    {
        GuiEvent = guiEvent;
        ElemId = elemId;
    }
}
