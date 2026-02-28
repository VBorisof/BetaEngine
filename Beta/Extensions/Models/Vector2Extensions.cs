using Microsoft.Xna.Framework;

namespace Beta.Extensions.Models;

public static class Vector2Extensions
{
    public static Vector2Surrogate ToSurrogate(this Vector2 vec)
    {
        return new Vector2Surrogate
        {
            X = vec.X,
            Y = vec.Y
        };
    }

    public static Vector2 ToVector2(this Vector2Surrogate sur)
    {
        return new Vector2
        {
            X = sur.X,
            Y = sur.Y
        };
    }
}