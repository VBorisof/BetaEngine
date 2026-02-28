using System;
using System.Collections.Generic;
using Beta.Gui;
using Beta.Gui.Elements;

namespace aced.Controllers;

internal static class ControllerBase
{
    public static GuiElement AddEntityListItem<TEntity>(
        TEntity entity,
        ListView<GuiElement> list,
        Dictionary<TEntity, Label> labelDict,
        Action onSelect,
        Action onDelete,
        Func<TEntity, string> labelTextSelector
    )
    {
        var item = Gui.Instance.LoadFromFiles(
            "Layouts/li-select-delete.xml",
            "Layouts/li-select-delete.css",
            list.Style.LayerDepth + Constants.LayerDepthStep
        );
        list.AddListItem(item);

        item.TryFindByClass<Label>("list-item-label", out var label);
        item.TryFindByClass<TextButton>("list-item-select-button", out var selectButton);
        item.TryFindByClass<TextButton>("list-item-delete-button", out var deleteButton);

        labelDict[entity] = label;
        label.Text = labelTextSelector(entity);

        selectButton.OnLeftClick += (_, _) =>
        {
            onSelect();
        };
        deleteButton.OnLeftClick += (_, _) =>
        {
            onDelete();
            list.RemoveListItem(item);
            labelDict.Remove(entity);
        };

        return item;
    }
}