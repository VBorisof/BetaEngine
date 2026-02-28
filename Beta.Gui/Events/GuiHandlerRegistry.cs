using System;
using System.Collections.Generic;

namespace Beta.Gui.Events;

public static class GuiHandlerRegistry
{
    private static Dictionary<IGuiHandler, List<GuiEventHandlerMapping>> _handlers = [];
    public static Dictionary<IGuiHandler, List<GuiEventHandlerMapping>> GetHandlers()
    {
        return _handlers;
    }

    public static void InvokeHandlers(GuiEventType guiEvent, string elemId)
    {
        foreach (var handler in _handlers)
        {
            foreach (var mapping in handler.Value)
            {
                if (mapping.GuiEventType == guiEvent
                    && string.Equals(mapping.ElemId, elemId, StringComparison.OrdinalIgnoreCase))
                {
                    mapping.Method.Invoke(handler.Key, null);
                }
            }
        }
    }

    public static void InvokeStringValueHandlers(GuiEventType guiEvent, string elemId, string value)
    {
        foreach (var handler in _handlers)
        {
            foreach (var mapping in handler.Value)
            {
                if (mapping.GuiEventType == guiEvent
                    && string.Equals(mapping.ElemId, elemId, StringComparison.OrdinalIgnoreCase))
                {
                    mapping.Method.Invoke(handler.Key, [value]);
                }
            }
        }
    }
    public static void InvokeIntValueHandlers(GuiEventType guiEvent, string elemId, int value)
    {
        foreach (var handler in _handlers)
        {
            foreach (var mapping in handler.Value)
            {
                if (mapping.GuiEventType == guiEvent
                    && string.Equals(mapping.ElemId, elemId, StringComparison.OrdinalIgnoreCase))
                {
                    mapping.Method.Invoke(handler.Key, [value]);
                }
            }
        }
    }
    public static void InvokeBoolHandlers(GuiEventType guiEvent, string elemId, bool value)
    {
        foreach (var handler in _handlers)
        {
            foreach (var mapping in handler.Value)
            {
                if (mapping.GuiEventType == guiEvent
                    && string.Equals(mapping.ElemId, elemId, StringComparison.OrdinalIgnoreCase))
                {
                    mapping.Method.Invoke(handler.Key, [value]);
                }
            }
        }
    }
}