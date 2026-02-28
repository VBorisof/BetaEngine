using Beta.Entities.Animations;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace aced.Models;

public class Costume
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Animation> Animations { get; set; } = [];
}

public class CostumeModel
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("animations")]
    public List<AnimationModel> Animations { get; set; } = [];
}

public class ActorData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("speed")]
    public float Speed { get; set; }
    [JsonPropertyName("costumes")]
    public List<CostumeModel> Costumes { get; set; }
    [JsonPropertyName("origin")]
    public Coord Origin { get; set; }
}
