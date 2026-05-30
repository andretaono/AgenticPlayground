using Game.Systems.Domain.Inventory.Ports;

namespace Game.Systems.Domain.Inventory.Controller;

internal sealed class InventoryController : IInventory
{
	private readonly List<IItem> _items = new();

	public int Capacity { get; }

	public InventoryController(int capacity)
	{
		if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
		Capacity = capacity;
	}

	public IReadOnlyList<IItem> Items => _items.AsReadOnly();

	public bool AddItem(IItem item)
	{
		if (_items.Count >= Capacity) return false;
		_items.Add(item);
		return true;
	}

	public bool RemoveItem(string itemId)
	{
		var index = FindItemIndex(itemId);
		if (index < 0) return false;
		_items.RemoveAt(index);
		return true;
	}

	public IItem? GetItem(string itemId)
	{
		var index = FindItemIndex(itemId);
		return index < 0 ? null : _items[index];
	}

	private int FindItemIndex(string itemId)
	{
		for (var i = 0; i < _items.Count; i++)
		{
			if (_items[i].Id == itemId)
				return i;
		}

		return -1;
	}

	public override string ToString()
	{
		if (_items.Count == 0) return "(empty)";
		return string.Join("\n", _items.Select(i => i.ToString()));
	}
}
