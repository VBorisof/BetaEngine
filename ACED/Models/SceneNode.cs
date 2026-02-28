using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneNode
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
}


