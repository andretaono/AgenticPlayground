using System.Collections.Generic;
using System.Linq;
using Game.Systems.Domain.Inventory.Interfaces;

namespace Game.Systems.Domain.Inventory.Core.Controller;

/// <summary>
/// Core inventory controller (plain C#, single-purpose, raises events for changes).
/// </summary>
public class InventoryController : IInventory
{
    private readonly List<IItem> _items = new();

    public int Capacity { get; }

    public InventoryController(int capacity = 20) => Capacity = capacity;

    public IReadOnlyList<IItem> Items => _items.AsReadOnly();

    public event EventHandler<ItemEventArgs>? ItemAdded;
    public event EventHandler<ItemEventArgs>? ItemRemoved;

    public bool AddItem(IItem item)
    {
        if (_items.Count >= Capacity) return false;
        _items.Add(item);
        ItemAdded?.Invoke(this, new ItemEventArgs(item));
        return true;
    }

    public bool RemoveItem(string itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return false;
        _items.Remove(item);
        ItemRemoved?.Invoke(this, new ItemEventArgs(item));
        return true;
    }

    public IItem? GetItem(string itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    public override string ToString() => !_items.Any() ? "(empty)" : string.Join("\n", _items.Select(i => i.ToString()));
}