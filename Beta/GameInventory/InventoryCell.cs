using Beta.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.GameInventory;

public enum InventoryCellClickIntent
{
    None,
    ExitInventory,
    PickItem
}

public record InventoryCellClickResult
{
    public required InventoryCellClickIntent Intent { get; init; }
    public required Actor? Actor { get; init; }
    public required int? Width { get; init; }
    public required int? Height { get; init; }
}

// TODO: Move to new GUI
public class InventoryCell
{
    public Actor? Item { get; set; }
    public Rectangle CellRectangle { get; private set; }
    private Vector2 _position;
    public bool IsDragging { get; set; }

    public Rectangle ItemRectangle { get; private set; }

    public InventoryCell(Rectangle rectangle)
    {
        CellRectangle = rectangle;
        _position = new Vector2(CellRectangle.X, CellRectangle.Y);
    }

    public void SetPosition(Vector2 invPosition)
    {
        CellRectangle = new Rectangle(
            (int)(invPosition.X + _position.X),
            (int)(invPosition.Y + _position.Y),
            CellRectangle.Width,
            CellRectangle.Height
        );

        var pos = new Vector2(CellRectangle.X, CellRectangle.Y);
        if (Item is not null)
        {
            // Is this something tall?
            int width, height;
            const int margin = 20;
            if (Item.GetBoundingRect().Height > Item.GetBoundingRect().Width)
            {
                height = CellRectangle.Height - margin;
                width = Item.GetBoundingRect().Width * height / Item.GetBoundingRect().Height;
            }
            // Is this something wide?
            else
            {
                width = CellRectangle.Width - margin;
                height = Item.GetBoundingRect().Height * width / Item.GetBoundingRect().Width;
            }
            ItemRectangle = new Rectangle(
                (int)(pos.X + ((CellRectangle.Width / 2) - (width / 2))),
                (int)(pos.Y + ((CellRectangle.Height / 2) - (height / 2))),
                width,
                height
            );
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Settings.IsDebug)
        {
            spriteBatch.DrawRectangle(
                CellRectangle.X,
                CellRectangle.Y,
                CellRectangle.Width,
                CellRectangle.Height,
                Color.Black,
                layerDepth: Constants.LayerDepthGui + (Constants.LayerDepthStep * 2)
            );
            spriteBatch.DrawRectangle(
                ItemRectangle.X,
                ItemRectangle.Y,
                ItemRectangle.Width,
                ItemRectangle.Height,
                Color.Green,
                layerDepth: Constants.LayerDepthGui + (Constants.LayerDepthStep * 2)
            );
        }

        if (!IsDragging && Item is not null)
        {
            if (Item?.CurrentAnimation is null)
            {
                return;
            }

            var texture = Item.CurrentAnimation.GetFirstFrame();
            spriteBatch.Draw(
                texture,
                destinationRectangle: ItemRectangle,
                sourceRectangle: texture.Bounds,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: Constants.LayerDepthGui + Constants.LayerDepthStep
            );
        }
    }
}