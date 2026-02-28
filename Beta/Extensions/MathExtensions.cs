using Microsoft.Xna.Framework;

namespace Beta.Extensions;

public static class MathExtensions
{
    public static float Dot(this Vector2 a, Vector2 b)
    {
        return (a.X*b.X) + (a.Y*b.Y);
    }
}


