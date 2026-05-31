using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Model;

public sealed class Item : IModifierSource
{
	internal Item(ItemId id, IReadOnlyList<Modifier> modifiers)
	{
		Id = id;
		Modifiers = modifiers;
	}

	public ItemId Id { get; }

	public IReadOnlyList<Modifier> Modifiers { get; }
}
