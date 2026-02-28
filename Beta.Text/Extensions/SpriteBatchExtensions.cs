using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Text.Extensions;

public static class SpriteBatchExtensions
{
    public static void DrawString(this SpriteBatch s, SpriteFont font, string text, Vector2 position, Color color, float layerDepth)
    {
        s.DrawString(
            font,
            text,
            position,
            color,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: layerDepth
        );
    }
}
