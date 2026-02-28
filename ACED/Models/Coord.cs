using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace aced.Models;

public class Coord
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }

    public Coord() { X = 0f; Y = 0f; }
    public Coord(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
    public static Coord FromVector2(Vector2 v)
    {
        return new Coord(v.X, v.Y);
    }
}

