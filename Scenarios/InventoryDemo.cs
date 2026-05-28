using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.Inventory.Core.Controller;
using Game.Systems.Domain.Inventory.Core.Model;
using Game.Systems.Domain.Inventory.Interfaces;

namespace Game.Scenarios;

/// <summary>
/// Console example demonstrating the core inventory system.
/// </summary>
public class InventoryDemo : IScenario
{
	public string Name => "inventory";

	public void Run()
    {
        var inventory = new InventoryController(capacity: 5);
        inventory.ItemAdded += (_, e) => Console.WriteLine($"[Event] Added: {e.Item.Name}");
        inventory.ItemRemoved += (_, e) => Console.WriteLine($"[Event] Removed: {e.Item.Name}");

        IItem potion = new ItemModel("potion_small", "Small Health Potion", "Restores 50 HP.", ItemType.Consumable);
        IItem sword = new ItemModel("iron_sword", "Iron Sword", "A basic iron sword.", ItemType.Equipment);
        IItem quest = new ItemModel("amulet_01", "Old Amulet", "Quest item", ItemType.Quest);

        Console.WriteLine($"Adding {potion.Name}: {inventory.AddItem(potion)}");
        Console.WriteLine($"Adding {sword.Name}: {inventory.AddItem(sword)}");
        Console.WriteLine($"Adding {quest.Name}: {inventory.AddItem(quest)}");

        Console.WriteLine("\nInventory contents:");
        Console.WriteLine(inventory);

        Console.WriteLine("\nRemoving sword...");
        inventory.RemoveItem(sword.Id);

        Console.WriteLine("\nInventory contents after removal:");
        Console.WriteLine(inventory);
    }
}