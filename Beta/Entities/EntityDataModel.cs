using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Entities.Costumes;
using Beta.Extensions.Models;

namespace Beta.Entities;

public class EntityDataModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("speed")]
    public float Speed { get; set; } = 1f;
    [JsonPropertyName("costumes")]
    public List<CostumeModel> Costumes { get; set; } = [];
    [JsonPropertyName("origin")]
    public Vector2Model Origin { get; set; } = Vector2Model.Zero;
}