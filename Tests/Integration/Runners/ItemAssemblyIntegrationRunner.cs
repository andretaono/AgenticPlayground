using Game.Systems.Domain.ItemAssembly;
using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Integration.Items;

namespace Game.Tests.Integration.Runners;

public sealed class ItemAssemblyIntegrationRunner
{
	public ItemAssemblyFlatResult RunFlatAggregation()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(socketCount: 2);

		var sword = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("Damage"), ModifierKind.Flat, Value: 25f),
			new Modifier(new ModifierId("AttackRange"), ModifierKind.Flat, Value: 1.5f)
		});

		var ring = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("Damage"), ModifierKind.Flat, Value: 10f)
		});

		system.Assembly.AddItem(assembly, socketIndex: 0, sword);
		system.Assembly.AddItem(assembly, socketIndex: 1, ring);
		var resolved = system.Resolver.Resolve(assembly);

		return new ItemAssemblyFlatResult(
			Damage: resolved.FlatValues[new ModifierId("Damage")],
			AttackRange: resolved.FlatValues[new ModifierId("AttackRange")],
			RawModifierCount: resolved.RawModifiers.Count);
	}

	public ItemAssemblyMixedResult RunMixedModifiers()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(socketCount: 1);

		var boots = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("MovementSpeed"), ModifierKind.Flat, Value: 2f),
			new Modifier(new ModifierId("MovementSpeed"), ModifierKind.Percent, Value: 0.15f),
			new Modifier(new ModifierId("CanSwim"), ModifierKind.Flag, Value: 1f)
		});

		system.Assembly.AddItem(assembly, socketIndex: 0, boots);
		var resolved = system.Resolver.Resolve(assembly);

		return new ItemAssemblyMixedResult(
			FlatMovementSpeed: resolved.FlatValues[new ModifierId("MovementSpeed")],
			PercentMovementSpeed: resolved.PercentValues[new ModifierId("MovementSpeed")],
			CanSwimEnabled: resolved.Flags.Contains(new ModifierId("CanSwim")));
	}

	public ItemAssemblyFlagResult RunFlagPriority()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(socketCount: 2);

		var curse = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("CanFly"), ModifierKind.Flag, Value: 0f, Priority: 1)
		});

		var wings = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("CanFly"), ModifierKind.Flag, Value: 1f, Priority: 10)
		});

		system.Assembly.AddItem(assembly, socketIndex: 0, curse);
		system.Assembly.AddItem(assembly, socketIndex: 1, wings);
		var resolved = system.Resolver.Resolve(assembly);

		return new ItemAssemblyFlagResult(
			CanFlyEnabled: resolved.Flags.Contains(new ModifierId("CanFly")));
	}

	public ItemAssemblyDeterminismResult RunDeterminism()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(socketCount: 2);

		var itemA = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("Damage"), ModifierKind.Flat, Value: 5f, Priority: 2),
			new Modifier(new ModifierId("Damage"), ModifierKind.Percent, Value: 0.1f, Priority: 1)
		});

		var itemB = system.ItemFactory.Create(new[]
		{
			new Modifier(new ModifierId("Damage"), ModifierKind.Flat, Value: 7f, Priority: 3)
		});

		system.Assembly.AddItem(assembly, socketIndex: 0, itemA);
		system.Assembly.AddItem(assembly, socketIndex: 1, itemB);

		var first = system.Resolver.Resolve(assembly);
		var second = system.Resolver.Resolve(assembly);

		var identical =
			first.FlatValues.SequenceEqual(second.FlatValues) &&
			first.PercentValues.SequenceEqual(second.PercentValues) &&
			first.Flags.OrderBy(id => id.Value).SequenceEqual(second.Flags.OrderBy(id => id.Value)) &&
			first.RawModifiers.SequenceEqual(second.RawModifiers);

		return new ItemAssemblyDeterminismResult(
			Identical: identical,
			FirstRawModifierCount: first.RawModifiers.Count);
	}

	public ItemAssemblyLootResult RunLootRandomizer()
	{
		var system = new ItemAssemblySystem();
		var catalog = new ModifierCatalog();
		var randomizer = new LootRandomizer(system.ItemFactory, catalog, new SeededRng(42));

		var drops = new List<Item>(5);
		for (var roll = 1; roll <= 5; roll++)
			drops.Add(randomizer.Roll(minModifiers: 1, maxModifiers: 3));

		var counts = new Dictionary<string, int>();
		for (var i = 0; i < 1000; i++)
		{
			var item = randomizer.Roll(minModifiers: 1, maxModifiers: 1);
			var id = item.Modifiers[0].Id.Value;
			counts[id] = counts.GetValueOrDefault(id) + 1;
		}

		var topModifier = counts.MaxBy(pair => pair.Value).Key;

		return new ItemAssemblyLootResult(
			AllDropsHaveModifiers: drops.All(item => item.Modifiers.Count >= 1),
			DistinctModifierTypesRolled: counts.Count,
			TopModifierId: topModifier,
			TopModifierRollCount: counts[topModifier]);
	}
}

public sealed record ItemAssemblyFlatResult(float Damage, float AttackRange, int RawModifierCount);

public sealed record ItemAssemblyMixedResult(
	float FlatMovementSpeed,
	float PercentMovementSpeed,
	bool CanSwimEnabled);

public sealed record ItemAssemblyFlagResult(bool CanFlyEnabled);

public sealed record ItemAssemblyDeterminismResult(bool Identical, int FirstRawModifierCount);

public sealed record ItemAssemblyLootResult(
	bool AllDropsHaveModifiers,
	int DistinctModifierTypesRolled,
	string TopModifierId,
	int TopModifierRollCount);
