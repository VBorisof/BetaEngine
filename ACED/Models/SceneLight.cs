using System.Text.Json.Serialization;
using Beta.Common.Extensions;
using Microsoft.Xna.Framework;

namespace aced.Models;

public record SceneLight
{
    [JsonPropertyName("lightType")]
    [JsonConverter(typeof(JsonStringEnumConverter<SceneLightType>))]
    public SceneLightType LightType { get; set; }
    [JsonPropertyName("lightPos")]
    public Vector3Model LightPosition { get; set; } = Vector3Model.Zero;
    [JsonPropertyName("lightIntensity")]
    public int LightIntensity { get; set; } = 10;

    private string _lightColorHex = "#ffffffff";
    [JsonPropertyName("lightColor")]
    public string LightColorHex
    {
        get => _lightColorHex;
        set
        {
            _lightColorHex = value;
            LightColor = ColorEx.FromHexString(_lightColorHex);
        }
    }

    [JsonIgnore]
    public Color LightColor { get; private set; }
}
