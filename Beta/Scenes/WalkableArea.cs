using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Scenes;

public record WalkableAreaModel
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("nodes")]
    public List<Vector2Model> Nodes { get; set; } = [];
}

public record SceneWalkableArea
{
    public required int Index { get; init; }
    public required List<Vector2> Nodes { get; init; }
    public required Polygon Polygon { get; init; }
}