using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.ItemAssembly;
using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Integration.Items;

namespace Game.Scenarios;

public sealed class ItemAssemblyDemo : IScenario
{
	public string Name => "item-assembly";

	public void Run()
	{
		RunFlatAggregationCase();
		Console.WriteLine();
		RunMixedModifierCase();
		Console.WriteLine();
		RunFlagPriorityCase();
		Console.WriteLine();
		RunDeterminismCase();
		Console.WriteLine();
		RunLootRandomizerCase();
	}

	private static void RunFlatAggregationCase()
	{
		Console.WriteLine("=== Case 1: Flat modifiers sum across socketed items ===");

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

		Console.WriteLine($"Socket 0: item {sword.Id} (+25 Damage, +1.5 AttackRange)");
		Console.WriteLine($"Socket 1: item {ring.Id} (+10 Damage)");
		Console.WriteLine($"Resolved Damage: {resolved.FlatValues[new ModifierId("Damage")]}");
		Console.WriteLine($"Resolved AttackRange: {resolved.FlatValues[new ModifierId("AttackRange")]}");
		Console.WriteLine($"Raw modifier count: {resolved.RawModifiers.Count}");
	}

	private static void RunMixedModifierCase()
	{
		Console.WriteLine("=== Case 2: Flat, percent, and flag modifiers ===");

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

		Console.WriteLine($"MovementSpeed (flat): {resolved.FlatValues[new ModifierId("MovementSpeed")]}");
		Console.WriteLine($"MovementSpeed (percent): {resolved.PercentValues[new ModifierId("MovementSpeed")]}");
		Console.WriteLine($"CanSwim enabled: {resolved.Flags.Contains(new ModifierId("CanSwim"))}");
	}

	private static void RunFlagPriorityCase()
	{
		Console.WriteLine("=== Case 3: Conflicting flags resolve by priority ===");

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

		Console.WriteLine("Curse: CanFly=false (priority 1)");
		Console.WriteLine("Wings: CanFly=true (priority 10)");
		Console.WriteLine($"CanFly enabled: {resolved.Flags.Contains(new ModifierId("CanFly"))}");
	}

	private static void RunDeterminismCase()
	{
		Console.WriteLine("=== Case 4: Same assembly produces identical resolved output ===");

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
			first.Flags.SetEquals(second.Flags) &&
			first.RawModifiers.SequenceEqual(second.RawModifiers);

		Console.WriteLine($"Deterministic resolve: {identical}");
		Console.WriteLine($"Raw modifier order: {string.Join(", ", first.RawModifiers.Select(modifier => $"{modifier.Id}:{modifier.Kind}@{modifier.Priority}"))}");
	}

	private static void RunLootRandomizerCase()
	{
		Console.WriteLine("=== Case 5: Weighted loot randomizer rolls items from ModifierCatalog ===");

		var system = new ItemAssemblySystem();
		var catalog = new ModifierCatalog();
		var randomizer = new LootRandomizer(system.ItemFactory, catalog, new SeededRng(42));

		Console.WriteLine("Rolling 5 loot drops (1-3 modifiers each):\n");

		for (var roll = 1; roll <= 5; roll++)
		{
			var item = randomizer.Roll(minModifiers: 1, maxModifiers: 3);
			Console.WriteLine($"Drop {roll} (item {item.Id}, {item.Modifiers.Count} modifiers):");

			foreach (var modifier in item.Modifiers)
				Console.WriteLine($"  - {modifier.Id} {modifier.Kind} {modifier.Value}");
		}

		Console.WriteLine("\nSpawn frequency over 1000 single-modifier rolls:");
		var counts = new Dictionary<string, int>();

		for (var i = 0; i < 1000; i++)
		{
			var item = randomizer.Roll(minModifiers: 1, maxModifiers: 1);
			var id = item.Modifiers[0].Id.Value;
			counts[id] = counts.GetValueOrDefault(id) + 1;
		}

		foreach (var entry in counts.OrderByDescending(pair => pair.Value))
			Console.WriteLine($"  {entry.Key}: {entry.Value}");
	}
}
