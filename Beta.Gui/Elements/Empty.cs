using Beta.Gui.Styles;

namespace Beta.Gui.Elements;

public class Empty : GuiElement
{
    public Empty(GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
    }

    public override string ToString()
    {
        return $"{nameof(Empty)} .{ElemClass} #{ElemId}";
    }
}