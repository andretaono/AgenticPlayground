using Game.Systems.Domain.Inventory;
using Game.Systems.Domain.Inventory.Model;
using Game.Systems.Domain.Inventory.Ports;

namespace Game.Tests.Integration.Runners;

public sealed class InventoryIntegrationRunner
{
	public InventoryIntegrationResult Run()
	{
		var inventory = new InventorySystem(capacity: 5).Inventory;

		IItem potion = new ItemModel("potion_small", "Small Health Potion", "Restores 50 HP.", ItemType.Consumable);
		IItem sword = new ItemModel("iron_sword", "Iron Sword", "A basic iron sword.", ItemType.Equipment);
		IItem quest = new ItemModel("amulet_01", "Old Amulet", "Quest item", ItemType.Quest);

		var addedPotion = inventory.AddItem(potion);
		var addedSword = inventory.AddItem(sword);
		var addedQuest = inventory.AddItem(quest);
		var countAfterAdds = inventory.Items.Count;

		var removedSword = inventory.RemoveItem(sword.Id);
		var countAfterRemoval = inventory.Items.Count;
		var stillHasPotion = inventory.GetItem(potion.Id) is not null;
		var stillHasQuest = inventory.GetItem(quest.Id) is not null;
		var swordGone = inventory.GetItem(sword.Id) is null;

		return new InventoryIntegrationResult(
			AddedPotion: addedPotion,
			AddedSword: addedSword,
			AddedQuest: addedQuest,
			CountAfterAdds: countAfterAdds,
			RemovedSword: removedSword,
			CountAfterRemoval: countAfterRemoval,
			StillHasPotion: stillHasPotion,
			StillHasQuest: stillHasQuest,
			SwordGone: swordGone);
	}
}

public sealed record InventoryIntegrationResult(
	bool AddedPotion,
	bool AddedSword,
	bool AddedQuest,
	int CountAfterAdds,
	bool RemovedSword,
	int CountAfterRemoval,
	bool StillHasPotion,
	bool StillHasQuest,
	bool SwordGone);
