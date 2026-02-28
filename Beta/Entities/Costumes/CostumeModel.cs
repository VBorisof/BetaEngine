using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Entities.Animations;

namespace Beta.Entities.Costumes;

public class CostumeModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("animations")]
    public List<AnimationModel> Animations { get; set; } = [];
}