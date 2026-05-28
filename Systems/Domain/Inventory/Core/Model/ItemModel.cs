using Game.Systems.Domain.Inventory.Interfaces;

namespace Game.Systems.Domain.Inventory.Core.Model;

/// <summary>
/// Immutable item model (core layer, no Unity dependency).
/// </summary>
public sealed record ItemModel(string Id, string Name, string Description, ItemType Type = ItemType.Misc, int Value = 0) : IItem
{
    public override string ToString() => $"{Name} (Type: {Type}, Value: {Value}) - {Description}";
}