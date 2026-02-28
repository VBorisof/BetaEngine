using System.Collections.Generic;
using Beta.Text;

namespace Beta.DI;

public class TextManagerModule
{
    public static FontBinding Debug { get; } = new(35, "betaf", "betaf_outline");
    public static FontBinding Main { get; } = new(35, "betaf", "betaf_outline", 35);
    public static FontBinding Hint { get; } = new(25, "hint", "hint_outline");

    public static TextManager MakeTextManager()
    {
        var fontBindings = new List<FontBinding>
        {
            Debug, Main, Hint
        };

        return new TextManager(fontBindings, Constants.LayerDepthText);
    }
}