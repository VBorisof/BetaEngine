using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Scenes;

public record RegionModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("timesActive")]
    public int TimesActive { get; set; }
    [JsonPropertyName("nodes")]
    public List<Vector2Model> Nodes { get; set; } = [];
}

public record SceneRegion
{
    public required string Name { get; init; }
    public required int TimesActive { get; init; }
    public required List<Vector2> Nodes { get; init; }
    public required Polygon Polygon { get; init; }
}