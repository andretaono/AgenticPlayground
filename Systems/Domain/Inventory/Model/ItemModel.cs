using Game.Systems.Domain.Inventory.Ports;

namespace Game.Systems.Domain.Inventory.Model;

public sealed record ItemModel(
	string Id,
	string Name,
	string Description,
	ItemType Type = ItemType.Misc,
	int Value = 0) : IItem
{
	public override string ToString() => $"{Name} (Type: {Type}, Value: {Value}) - {Description}";
}
