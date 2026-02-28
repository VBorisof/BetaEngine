using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Scenes;

public record ExitModel
{
    [JsonPropertyName("index")]
    public int StartIndex { get; set; }
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;
    [JsonPropertyName("targetIndex")]
    public int TargetIndex { get; set; }
    [JsonPropertyName("exitPoint")]
    public Vector2Model ExitPoint { get; set; } = Vector2Model.Zero;
    [JsonPropertyName("nodes")]
    public List<Vector2Model> Nodes { get; set; } = [];
}

public record SceneExit
{
    public required int StartIndex { get; init; }
    public required string Destination { get; init; }
    public required int TargetIndex { get; init; }
    public required Vector2 ExitPoint { get; init; }
    public required List<Vector2> Nodes { get; init; }
    public required Polygon Polygon { get; init; }
}