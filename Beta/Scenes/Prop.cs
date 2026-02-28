using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Beta.Verbs;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Scenes;

public record PropModel
{
    [JsonPropertyName("name")]
    public string DeclName { get; set; } = string.Empty;
    [JsonPropertyName("nodes")]
    public List<Vector2Model> Nodes { get; set; } = [];
}

public class SceneProp
{
    public required string DeclName { get; init; }
    public required List<Vector2> Nodes { get; init; }

    public required string Name { get; set; }
    public required Polygon Polygon { get; init; }

    public Dictionary<Verb, EventHandler> VerbHandlers { get; } = [];

    public SceneProp()
    {
        foreach (var verb in Enum.GetValues<Verb>())
        {
            VerbHandlers[verb] = (_, __) => { };
        }
    }

    public bool Contains(Vector2 pos)
    {
        return Polygon.Contains(pos);
    }
}