using System.Collections.Generic;
using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Beta.Gui.Elements;

public class ListView<T> : GuiElement where T : GuiElement
{
    private int _startIndex;
    private readonly int _numVisibleItems;
    private readonly Box _box;
    private readonly TextButton _scrollUpButton;
    private readonly TextButton _scrollDownButton;

    private const int ItemWidthMargin = 10;
    private const int ButtonSize = 20;

    private readonly List<GuiElement> _listItems = [];

    public int Count => _listItems.Count;

    public ListView(GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        _numVisibleItems = (int)Style.Size.Height / Style.ChildItemHeight;

        _box = new Box(style with
        {
            RelativePosition = new Vector2(0, 0)
        }, extraInputContexts);

        _scrollUpButton = new TextButton("^", style with
        {
            Size = new SizeF(ButtonSize, ButtonSize),
            RelativePosition = new Vector2(Style.Size.Width - ButtonSize, 0)
        }, extraInputContexts);
        _scrollUpButton.SubscribeOnLeftClick((_, __) =>
        {
            ScrollUp();
        });

        _scrollDownButton = new TextButton("v", style with
        {
            Size = new SizeF(ButtonSize, ButtonSize),
            RelativePosition = new Vector2(Style.Size.Width - ButtonSize, Style.Size.Height - ButtonSize)
        }, extraInputContexts);
        _scrollDownButton.SubscribeOnLeftClick((_, __) =>
        {
            ScrollDown();
        });

        _box.AddElement(_scrollUpButton);
        _box.AddElement(_scrollDownButton);
        AddElement(_box);
    }

    public void AddListItem(T item)
    {
        item.Style.Size = new SizeF(
            Style.Size.Width - ButtonSize - ItemWidthMargin * 2,
            Style.ChildItemHeight
        );
        item.Style.LayerDepth = _box.Style.LayerDepth + Constants.LayerDepthStep;
        _listItems.Add(item);
        _box.AddElement(item);
        
        RepositionItems();
        ScrollDown();
    }
    public void RemoveListItem(T item)
    {
        _listItems.Remove(item);
        _box.RemoveElement(item);
        ScrollUp();
        RepositionItems();
    }

    public void ScrollDown()
    {
        if (_listItems.Count < _numVisibleItems)
        {
            return;
        }

        ++_startIndex;
        if (_startIndex + _numVisibleItems > _listItems.Count)
        {
            _startIndex = 0;
        }

        RepositionItems();
    }

    public void ScrollUp()
    {
        if (_listItems.Count < _numVisibleItems)
        {
            return;
        }

        --_startIndex;
        if (_startIndex < 0)
        {
            _startIndex = _listItems.Count - _numVisibleItems;
        }

        RepositionItems();
    }

    private void RepositionItems()
    {
        for (var i = 0; i < _listItems.Count; ++i)
        {
            if (i < _startIndex)
            {
                _listItems[i].Style.Hidden = true;
                continue;
            }
            else if (i >= _startIndex + _numVisibleItems)
            {
                _listItems[i].Style.Hidden = true;
                continue;
            }
            else
            {
                _listItems[i].Style.RelativePosition =
                        new Vector2(ItemWidthMargin, (i - _startIndex) * Style.ChildItemHeight);
                _listItems[i].Style.Hidden = false;
            }
        }
    }

    public override string ToString()
    {
        return $"{nameof(ListView<T>)} .{ElemClass} #{ElemId}";
    }
}