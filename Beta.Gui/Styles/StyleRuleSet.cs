using System.Collections.Generic;

namespace Beta.Gui.Styles;

public class StyleRuleSet
{
    public string Selector { get; set; } = "";
    public Dictionary<string, string> Declarations { get; set; } = [];
}