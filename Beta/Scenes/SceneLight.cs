using System.Text.Json.Serialization;
using Beta.Extensions.Models;

namespace Beta.Scenes;

public record SceneLightModel
{
    [JsonPropertyName("lightType")]
    [JsonConverter(typeof(JsonStringEnumConverter<SceneLightType>))]
    public SceneLightType LightType { get; set; }
    [JsonPropertyName("lightPos")]
    public Vector3Model LightPosition { get; set; } = Vector3Model.Zero;
    [JsonPropertyName("lightColor")]
    public string LightColor { get; set; } = "#ffffffff";
    [JsonPropertyName("lightIntensity")]
    public int LightIntensity { get; set; } = 1;
}