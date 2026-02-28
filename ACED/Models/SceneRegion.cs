using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneRegion : ISceneNodeList
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("timesActive")]
    public int TimesActive { get; set; } = 0;
    [JsonPropertyName("nodes")]
    public List<SceneNode> Nodes { get; set; } = [];
}