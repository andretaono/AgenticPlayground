using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Integration.Items;

public readonly record struct CatalogModifier(
	ModifierId Id,
	ModifierKind Kind,
	ModifierDomain Domain,
	float Value = 0f,
	int Priority = 0,
	float Weight = 1f)
{
	public Modifier ToModifier() => new(Id, Kind, Value, Priority);
}
