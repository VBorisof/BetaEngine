using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace aced.Models;

public class WalkableArea : ISceneNodeList
{
    // TODO: Remove this probably.
    [JsonPropertyName("index")]
    public int Index { get; set; } = 0;

    [JsonPropertyName("nodes")]
    public List<SceneNode> Nodes { get; set; } = [];
}
