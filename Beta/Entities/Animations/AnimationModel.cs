using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Beta.Entities.Animations;

public class AnimationModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("speed")]
    public float Speed { get; set; }
    [JsonPropertyName("repeat")]
    public bool Repeat { get; set; }
    [JsonPropertyName("frames")]
    public List<string> Frames { get; set; } = [];
}
