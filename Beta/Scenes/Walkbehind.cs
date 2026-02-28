using Microsoft.Xna.Framework.Graphics;

namespace Beta.Scenes;

public class Walkbehind
{
    public Texture2D Texture { get; set; }
    public int Baseline { get; set; }
    public float LayerDepth { get; set; }

    public Walkbehind(Texture2D texture, int baseline)
    {
        Texture = texture;
        Baseline = baseline;
    }
}


