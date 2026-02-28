using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Common.Extensions;

public static class SpriteBatchExtensions
{
    public static void DrawRectBorder(
        this SpriteBatch spriteBatch,
        float rectWidth,
        float rectHeight,
        Vector2 rectPosition,
        float layerDepth,
        Color? color = null,
        int margin = 1
    )
    {
        if (color == null)
        {
            color = Color.White;
        }

        var topLeft = rectPosition + new Vector2(-margin, -margin);
        var topRight = rectPosition + new Vector2(rectWidth + margin, -margin);
        var bottomLeft = rectPosition + new Vector2(-margin * 2, rectHeight + margin);
        var bottomRight = rectPosition + new Vector2(rectWidth + margin, rectHeight + margin);

        // TOP
        spriteBatch.DrawLine(
            topLeft, topRight,
            color.Value,
            layerDepth: layerDepth
        );
        // LEFT
        spriteBatch.DrawLine(
            topLeft, bottomLeft,
            color.Value,
            layerDepth: layerDepth
        );
        // BOTTOM
        spriteBatch.DrawLine(
            bottomLeft, bottomRight,
            color.Value,
            layerDepth: layerDepth
        );
        // RIGHT
        spriteBatch.DrawLine(
            topRight, bottomRight,
            color.Value,
            layerDepth: layerDepth
        );
    }
}