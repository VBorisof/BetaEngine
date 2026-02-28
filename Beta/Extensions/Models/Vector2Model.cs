using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace Beta.Extensions.Models;

public class Vector2Model
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public static Vector2Model Zero { get; } = new()
    {
        X = 0,
        Y = 0
    };
}
