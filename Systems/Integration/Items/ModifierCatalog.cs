using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Integration.Items;

public sealed class ModifierCatalog
{
	public IReadOnlyList<CatalogModifier> All { get; } =
	[
		new(
			new ModifierId("ground_health"),
			ModifierKind.Flat,
			ModifierDomain.Ground,
			Value: 10f,
			Weight: 10f),

		new(
			new ModifierId("ground_stability"),
			ModifierKind.Percent,
			ModifierDomain.Ground,
			Value: 0.15f,
			Weight: 8f),

		new(
			new ModifierId("aerial_speed"),
			ModifierKind.Percent,
			ModifierDomain.Aerial,
			Value: 0.20f,
			Weight: 6f),

		new(
			new ModifierId("aerial_flight"),
			ModifierKind.Flag,
			ModifierDomain.Aerial,
			Weight: 2f),

		new(
			new ModifierId("oceanic_swim"),
			ModifierKind.Flag,
			ModifierDomain.Oceanic,
			Weight: 5f),

		new(
			new ModifierId("oceanic_breath"),
			ModifierKind.Flag,
			ModifierDomain.Oceanic,
			Weight: 3f),

		new(
			new ModifierId("beast_damage"),
			ModifierKind.Flat,
			ModifierDomain.Beast,
			Value: 5f,
			Weight: 10f),

		new(
			new ModifierId("beast_leap"),
			ModifierKind.Percent,
			ModifierDomain.Beast,
			Value: 0.25f,
			Weight: 7f),

		new(
			new ModifierId("spirit_regeneration"),
			ModifierKind.Flat,
			ModifierDomain.Spirit,
			Value: 2f,
			Weight: 6f),

		new(
			new ModifierId("spirit_phase"),
			ModifierKind.Flag,
			ModifierDomain.Spirit,
			Weight: 2f),

		new(
			new ModifierId("stone_armor"),
			ModifierKind.Flat,
			ModifierDomain.Stone,
			Value: 15f,
			Weight: 5f),

		new(
			new ModifierId("stone_resilience"),
			ModifierKind.Percent,
			ModifierDomain.Stone,
			Value: 0.10f,
			Weight: 5f),

		new(
			new ModifierId("shadow_stealth"),
			ModifierKind.Flag,
			ModifierDomain.Shadow,
			Weight: 2f),

		new(
			new ModifierId("shadow_critical"),
			ModifierKind.Percent,
			ModifierDomain.Shadow,
			Value: 0.15f,
			Weight: 4f)
	];
}
