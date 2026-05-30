namespace Game.Systems.Domain.Inventory.Ports;

public enum ItemType
{
	Consumable,
	Equipment,
	Quest,
	Misc
}

public interface IItem
{
	string Id { get; }
	string Name { get; }
	string Description { get; }
	ItemType Type { get; }
	int Value { get; }
}
