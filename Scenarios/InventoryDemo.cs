using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.Inventory;
using Game.Systems.Domain.Inventory.Model;
using Game.Systems.Domain.Inventory.Ports;

namespace Game.Scenarios;

/// <summary>
/// Console example demonstrating the core inventory system.
/// </summary>
public class InventoryDemo : IScenario
{
	public string Name => "inventory";

	public void Run()
    {
        var inventory = new InventorySystem(capacity: 5).Inventory;

        IItem potion = new ItemModel("potion_small", "Small Health Potion", "Restores 50 HP.", ItemType.Consumable);
        IItem sword = new ItemModel("iron_sword", "Iron Sword", "A basic iron sword.", ItemType.Equipment);
        IItem quest = new ItemModel("amulet_01", "Old Amulet", "Quest item", ItemType.Quest);

        Console.WriteLine($"Adding {potion.Name}: {inventory.AddItem(potion)}");
        if (inventory.GetItem(potion.Id) is not null)
            Console.WriteLine($"[Added] {potion.Name}");
        Console.WriteLine($"Adding {sword.Name}: {inventory.AddItem(sword)}");
        if (inventory.GetItem(sword.Id) is not null)
            Console.WriteLine($"[Added] {sword.Name}");
        Console.WriteLine($"Adding {quest.Name}: {inventory.AddItem(quest)}");
        if (inventory.GetItem(quest.Id) is not null)
            Console.WriteLine($"[Added] {quest.Name}");

        Console.WriteLine("\nInventory contents:");
        Console.WriteLine(inventory);

        Console.WriteLine("\nRemoving sword...");
        if (inventory.RemoveItem(sword.Id))
            Console.WriteLine($"[Removed] {sword.Name}");

        Console.WriteLine("\nInventory contents after removal:");
        Console.WriteLine(inventory);
    }
}