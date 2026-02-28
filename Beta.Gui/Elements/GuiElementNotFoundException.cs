using System;

namespace Beta.Gui.Elements;

public class GuiElementNotFoundException : Exception
{
    public GuiElementNotFoundException()
    {
    }

    public GuiElementNotFoundException(string? message) : base(message)
    {
    }
}
