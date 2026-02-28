using System;
using System.Collections.Generic;
using Beta.Actors;

namespace Beta.GameInventory;

public class Inventory
{
    public InventoryView View { get; }
    public List<Actor> Items { get; } = [];

    public EventHandler<Actor> OnItemAdded { get; set; } = (_, _) => { };
    public EventHandler<Actor> OnItemRemoved { get; set; } = (_, _) => { };

    public Inventory()
    {
        View = new InventoryView(this);
    }

    public void AddItem(Actor item)
    {
        Items.Add(item);
        OnItemAdded(this, item);
    }
    public void RemoveItem(Actor item)
    {
        if (!Items.Contains(item))
        {
            return;
        }
        Items.Remove(item);
        OnItemRemoved(this, item);
    }
    public void Clear()
    {
        for (var i = Items.Count - 1; i >= 0; --i)
        {
            RemoveItem(Items[i]);
        }
    }
}