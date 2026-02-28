using System.Collections.Generic;
using System.Linq;
using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Beta.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Beta.Services.Sounds;
using Beta.Common;

namespace Beta.GameInventory;

public class InventoryView
{
    public int Width { get; } = 1200;
    public int Height { get; } = 694;

    private const int CellSize = 140;

    private Vector2 _inventoryPos = Vector2.Zero;
    private readonly Texture2D _inventoryTexture;
    private readonly ILogger _logger;
    private readonly OrthographicCamera _camera;
    private readonly Inventory _inventory;
    private readonly List<InventoryCell> _cells;
    private readonly ITextManager _textManager;

    private Vector2 _mousePosScene = Vector2.Zero;
    private InventoryCell? _hoveredCell;
    private readonly SoundService _soundService;

    public InventoryView(Inventory inventory)
    {
        _soundService = DependencyContainer.Instance.Get<SoundService>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _camera = DependencyContainer.Instance.Get<OrthographicCamera>();
        _inventory = inventory;
        _cells = [];

        float xPadding = 40f;
        float yPadding = 40f;
        int cols = (int)((Width - (xPadding * 2)) / CellSize);
        int rows = (int)((Height - (yPadding * 2)) / CellSize);
        for (int y = 0; y < rows; ++y)
        {
            for (int x = 0; x < cols; ++x)
            {
                _cells.Add(new InventoryCell(
                    new Rectangle(
                        (int)xPadding + (x * CellSize),
                        (int)yPadding + (y * CellSize),
                        CellSize,
                        CellSize)
                ));
            }
        }

        _inventoryTexture = DependencyContainer.Instance.Get<ContentCache>().Get<Texture2D>("img/ui/inventory");

        _inventory.OnItemAdded += OnItemAdded;
        _inventory.OnItemRemoved += OnItemRemoved;

        _textManager = DependencyContainer.Instance.Get<ITextManager>();
    }

    public void OnItemAdded(object? sender, Actor item)
    {
        // Add item to the next free cell
        // TODO: Consider the size of the inventory. Do we need extra pages?
        var bestFreeCell = _cells
            .Where(c => c.Item == null)
            .OrderBy(c => c.CellRectangle.Y)
            .ThenBy(c => c.CellRectangle.X)
            .First();

        bestFreeCell.Item = item;
    }

    public void OnItemRemoved(object? sender, Actor item)
    {
        var cell = _cells.First(c => c.Item == item);
        cell.Item = null;
    }

    public bool IsOutsideInventory(Vector2 mousePosScreen)
    {
        var tolerance = 30;
        return mousePosScreen.X < _inventoryPos.X - tolerance
            || mousePosScreen.X > _inventoryPos.X + Width + tolerance
            || mousePosScreen.Y < _inventoryPos.Y - tolerance
            || mousePosScreen.Y > _inventoryPos.Y + Height + tolerance;
    }

    public InventoryCellClickResult OnClick(Vector2 mousePosScreen)
    {
        if (IsOutsideInventory(mousePosScreen))
        {
            return new InventoryCellClickResult
            {
                Intent = InventoryCellClickIntent.ExitInventory,
                Actor = null,
                Width = null,
                Height = null,
            };
        }

        var cell = GetFilledCellAtPositionOrDefault(mousePosScreen);
        if (cell is null)
        {
            return new InventoryCellClickResult
            {
                Intent = InventoryCellClickIntent.None,
                Actor = null,
                Width = null,
                Height = null,
            };
        }

        _logger.Debug($"Select {cell.Item}");
        _soundService.PlaySound(GameSoundType.InventoryPick);

        cell.IsDragging = true;
        _hoveredCell = null;
        return new InventoryCellClickResult
        {
            Intent = InventoryCellClickIntent.PickItem,
            Actor = cell.Item,
            Width = cell.ItemRectangle.Width,
            Height = cell.ItemRectangle.Height
        };
    }

    public void OnMoveCursor(Vector2 mousePosScene)
    {
        _mousePosScene = mousePosScene;
        var lastHoveredCell = _hoveredCell;
        _hoveredCell = GetFilledCellAtPositionOrDefault(mousePosScene);
        if (lastHoveredCell != _hoveredCell && _hoveredCell?.Item is not null)
        {
            _soundService.PlaySound(GameSoundType.Hover);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture: _inventoryTexture,
            sourceRectangle: _inventoryTexture.Bounds,
            destinationRectangle: new RectangleF(_inventoryPos.X, _inventoryPos.Y, Width, Height).ToRectangle(),
            color: Color.White * 0.85f,
            origin: Vector2.Zero,
            rotation: 0,
            effects: SpriteEffects.None,
            layerDepth: Constants.LayerDepthGui
        );

        foreach (var cell in _cells)
        {
            cell.Draw(spriteBatch);
        }

        if (_hoveredCell is not null && _hoveredCell.Item is not null)
        {
            _textManager.WriteLine(
                spriteBatch,
                _hoveredCell.Item.Name,
                new TextWriteArgs
                {
                    FontBinding = TextManagerModule.Hint,
                    Position = _mousePosScene - new Vector2(0, 100),
                    Color = Color.White,
                    TextAlignment = TextAlignment.Center,
                }
            );
        }
    }

    public Actor? GetAtScreenPosOrDefault(Vector2 screenPos)
    {
        var cell = _cells.FirstOrDefault(c =>
            c.CellRectangle.Contains(screenPos));

        return cell?.Item;
    }

    public InventoryCell? GetFilledCellAtPositionOrDefault(Vector2 mousePosScreen)
    {
        var cell = _cells.FirstOrDefault(c =>
            c.CellRectangle.Contains(mousePosScreen));

        return cell is null || cell.Item is null ? null : cell;
    }

    public void SetPosition(Vector2 position)
    {
        _inventoryPos = position;
        foreach (var cell in _cells)
        {
            cell.SetPosition(_inventoryPos);
        }
    }

    public void Reset()
    {
        var items = new List<Actor>();

        foreach (var cell in _cells)
        {
            cell.IsDragging = false;
            if (cell.Item != null)
            {
                items.Add(cell.Item);
                cell.Item = null;
            }
        }

        foreach (var item in items)
        {
            var bestFreeCell = _cells
                .Where(c => c.Item == null)
                .OrderBy(c => c.CellRectangle.Y)
                .ThenBy(c => c.CellRectangle.X)
                .First();

            bestFreeCell.Item = item;
        }
    }
}
