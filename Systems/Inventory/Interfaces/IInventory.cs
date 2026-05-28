using System;
using System.Collections.Generic;

namespace Game.Inventory.Interfaces;

public class ItemEventArgs : EventArgs
{
    public IItem Item { get; }
    public ItemEventArgs(IItem item) => Item = item;
}

public interface IInventory
{
    int Capacity { get; }
    IReadOnlyList<IItem> Items { get; }

    bool AddItem(IItem item);
    bool RemoveItem(string itemId);
    IItem? GetItem(string itemId);

    event EventHandler<ItemEventArgs>? ItemAdded;
    event EventHandler<ItemEventArgs>? ItemRemoved;
}