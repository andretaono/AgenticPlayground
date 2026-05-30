using Game.Systems.Domain.Inventory.Controller;
using Game.Systems.Domain.Inventory.Ports;

namespace Game.Systems.Domain.Inventory;

/// <summary>
/// Root orchestrator for inventory operations.
/// </summary>
public sealed class InventorySystem : IInventorySystem
{
	public IInventory Inventory { get; }

	public InventorySystem(int capacity = 20)
	{
		Inventory = new InventoryController(capacity);
	}
}
