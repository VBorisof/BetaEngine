using System.Globalization;
using Microsoft.Xna.Framework;

namespace Beta.Common.Extensions;

public static class ColorEx
{
    public static Color FromHexString(string s)
    {
        var colorstr = s.Trim('#');
        var r = int.Parse($"{colorstr[0]}{colorstr[1]}", NumberStyles.HexNumber);
        var g = int.Parse($"{colorstr[2]}{colorstr[3]}", NumberStyles.HexNumber);
        var b = int.Parse($"{colorstr[4]}{colorstr[5]}", NumberStyles.HexNumber);
        var a = 256f;
        if (colorstr.Length == 8)
        {
            a = int.Parse($"{colorstr[6]}{colorstr[7]}", NumberStyles.HexNumber);
        }

        return new Color(r, g, b) * (a / 256f);
    }
}
