using Microsoft.Xna.Framework;

namespace Beta.Lights;

public record PointLight
{
    public required Vector3 Color { get; set; }
    public required Vector3 Position { get; set; }
    public required int Intensity { get; set; }
}