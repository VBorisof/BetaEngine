using Beta.Gui.Styles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Beta.Gui.Elements;

public class DropdownMenu : GuiElement
{
    private readonly List<GuiElement> _elementList = [];

    public bool IsOpen { get; private set; }

    public DropdownMenu(string name, GuiElementStyle style, string? extraInputContexts) : base(style, extraInputContexts)
    {
        var mainButton = new TextButton(name, style with
        {
            RelativePosition = Vector2.Zero
        }, extraInputContexts);
        mainButton.SubscribeOnLeftClick((_, __) => Toggle());
        base.AddElement(mainButton);
    }

    public override void AddElement(GuiElement element)
    {
        element.Style = Style with
        {
            RelativePosition = new Vector2(int.MinValue, Style.ChildItemHeight * (_elementList.Count + 1))
        };
        _elementList.Add(element);
        base.AddElement(element);
    }

    public TextButton AddButton(string name)
    {
        var button = new TextButton(name, Style with
        {
            RelativePosition = new Vector2(int.MinValue, Style.ChildItemHeight * (_elementList.Count + 1))
        }, ExtraInputContexts);
        _elementList.Add(button);
        AddElement(button);

        return button;
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;

        if (IsOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        foreach (var item in _elementList)
        {
            item.Style.RelativePosition = new Vector2(0, item.Style.RelativePosition.Y);
        }

        IsOpen = true;
    }

    public void Close()
    {
        foreach (var item in _elementList)
        {
            item.Style.RelativePosition = new Vector2(int.MinValue, item.Style.RelativePosition.Y);
        }

        IsOpen = false;
    }

    public override string ToString()
    {
        return $"{nameof(DropdownMenu)} .{ElemClass} #{ElemId}";
    }
}