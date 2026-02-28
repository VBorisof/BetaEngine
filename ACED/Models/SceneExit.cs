using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace aced.Models;

public class SceneExit : ISceneNodeList
{
    [JsonPropertyName("index")]
    public int Index { get; set; } = -1;
    [JsonPropertyName("targetIndex")]
    public int TargetIndex { get; set; } = -1;
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = "";
    [JsonPropertyName("exitPoint")]
    public Coord ExitPoint { get; set; } = new Coord();
    [JsonPropertyName("nodes")]
    public List<SceneNode> Nodes { get; set; } = [];
}
