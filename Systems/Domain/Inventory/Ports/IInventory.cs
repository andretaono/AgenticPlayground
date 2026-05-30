using Game.Systems.Domain.Inventory.Ports;

namespace Game.Systems.Domain.Inventory.Ports;

public interface IInventory
{
	int Capacity { get; }
	IReadOnlyList<IItem> Items { get; }

	bool AddItem(IItem item);
	bool RemoveItem(string itemId);
	IItem? GetItem(string itemId);
}

public interface IInventorySystem
{
	IInventory Inventory { get; }
}
