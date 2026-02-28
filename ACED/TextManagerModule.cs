using Beta.Text;
using System.Collections.Generic;

namespace aced;

public class TextManagerModule
{
    public static FontBinding Main { get; } = new(20, "ubuntu", "ubuntu");

    public TextManager MakeTextManager()
    {
        var fontBindings = new List<FontBinding>
        {
            Main
        };

        return new TextManager(fontBindings, 0.9f);
    }
}