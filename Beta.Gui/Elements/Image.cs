using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class Image : GuiElement
{
    private Texture2D _texture;

    public Image(Texture2D texture, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _texture = texture;

        SubscribeOnHover((_, _) => { });
        SubscribeOnHoverEnd((_, _) => { });
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (Style.Hidden)
        {
            return;
        }

        spriteBatch.Draw(
            _texture,
            destinationRectangle: new RectangleF(GetAbsolutePosition(), Style.Size).ToRectangle(),
            sourceRectangle: _texture.Bounds,
            color: IsHovered ? Style.HoverColor : Style.Color,
            rotation: 0f,
            origin: Vector2.Zero,
            effects: SpriteEffects.None,
            layerDepth: Style.LayerDepth
        );
    }

    public override string ToString()
    {
        return $"{nameof(Image)} .{ElemClass} #{ElemId}";
    }
}