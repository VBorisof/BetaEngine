using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace aced.Models;

public class SceneData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("texturePath")]
    public string TexturePath { get; set; }
    [JsonPropertyName("walkableAreas")]
    public List<WalkableArea> WalkableAreas { get; set; } = [];
    [JsonPropertyName("exits")]
    public List<SceneExit> Exits { get; set; } = [];
    [JsonPropertyName("regions")]
    public List<SceneRegion> Regions { get; set; } = [];
    [JsonPropertyName("props")]
    public List<SceneProp> Props { get; set; } = [];
    [JsonPropertyName("walkbehinds")]
    public List<SceneWalkbehind> Walkbehinds { get; set; } = [];
    [JsonPropertyName("lights")]
    public List<SceneLight> Lights { get; set; } = [];
    [JsonPropertyName("scaleMap")]
    public SceneScaleMap ScaleMap { get; set; }
    [JsonPropertyName("actors")]
    public List<SceneActor> Actors { get; set; } = [];
}