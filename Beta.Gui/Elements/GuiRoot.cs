using Beta.Gui.Styles;

namespace Beta.Gui.Elements;

public class GuiRoot : GuiElement
{
    public GuiRoot(GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
    }

    public override string ToString()
    {
        return $"{nameof(GuiRoot)} .{ElemClass} #{ElemId}";
    }
}