using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace Beta.Extensions.Models;

public class Vector3Model
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }
    [JsonPropertyName("z")]
    public float Z { get; set; }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public static Vector3Model Zero { get; } = new()
    {
        X = 0,
        Y = 0,
        Z = 0
    };
}
