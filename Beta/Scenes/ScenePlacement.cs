using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Microsoft.Xna.Framework;

namespace Beta.Scenes;

public record ScenePlacementModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("position")]
    public Vector2Model Position { get; set; } = Vector2Model.Zero;
    [JsonPropertyName("scale")]
    public float Scale { get; set; }
    [JsonPropertyName("isShowChildren")]
    public bool IsShowChildren { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
    [JsonPropertyName("children")]
    public List<ScenePlacementModel> Children { get; set; } = [];
}

public record ScenePlacement
{
    public required string Name { get; init; }
    public required Vector2 Position { get; init; }
    public required float Scale { get; init; }
    public required bool IsShowChildren { get; init; }
    public required string State { get; init; }
    public required List<ScenePlacement> Children { get; init; }

    public static ScenePlacement FromScenePlacementModel(ScenePlacementModel model)
    {
        return new ScenePlacement
        {
            Name = model.Name,
            Position = model.Position.ToVector2(),
            Scale = model.Scale,
            State = model.State,
            IsShowChildren = model.IsShowChildren,
            Children = model.Children.Select(c => FromScenePlacementModel(c)).ToList()
        };
    }
}