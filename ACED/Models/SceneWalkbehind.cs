using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneWalkbehind : ISceneNodeList
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("nodes")]
    public List<SceneNode> Nodes { get; set; } = [];
}
