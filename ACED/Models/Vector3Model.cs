using System.Text.Json.Serialization;

namespace aced.Models;

public record Vector3Model
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }
    [JsonPropertyName("z")]
    public float Z { get; set; }

    public static Vector3Model Zero => new()
    {
        X = 0,
        Y = 0,
        Z = 0
    };
}

