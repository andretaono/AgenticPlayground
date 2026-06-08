using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.ItemAssembly.Tests;

public sealed class ItemAssemblyTests : ITestSuite
{
	public string Name => "unit/item-assembly";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "sums flat modifiers with the same id", SumFlatModifiers);
		registry.Add(Name, "sums percent modifiers with the same id", SumPercentModifiers);
		registry.Add(Name, "flag conflict resolves by higher priority", FlagConflictByPriority);
		registry.Add(Name, "flags with non-positive value are excluded", FlagsExcludeNonPositive);
	}

	private static void SumFlatModifiers()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(1);
		var damageId = new ModifierId("damage");

		var item = system.ItemFactory.Create(new[]
		{
			new Modifier(damageId, ModifierKind.Flat, 3f),
			new Modifier(damageId, ModifierKind.Flat, 5f)
		});

		system.Assembly.AddItem(assembly, 0, item);
		var resolved = system.Resolver.Resolve(assembly);

		TestAssert.Equal(8f, resolved.FlatValues[damageId]);
	}

	private static void SumPercentModifiers()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(1);
		var speedId = new ModifierId("speed");

		var item = system.ItemFactory.Create(new[]
		{
			new Modifier(speedId, ModifierKind.Percent, 10f),
			new Modifier(speedId, ModifierKind.Percent, 15f)
		});

		system.Assembly.AddItem(assembly, 0, item);
		var resolved = system.Resolver.Resolve(assembly);

		TestAssert.Equal(25f, resolved.PercentValues[speedId]);
	}

	private static void FlagConflictByPriority()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(2);
		var flagId = new ModifierId("on-fire");

		var lowPriorityItem = system.ItemFactory.Create(new[]
		{
			new Modifier(flagId, ModifierKind.Flag, 1f, Priority: 1)
		});
		var highPriorityItem = system.ItemFactory.Create(new[]
		{
			new Modifier(flagId, ModifierKind.Flag, 0f, Priority: 5)
		});

		system.Assembly.AddItem(assembly, 0, lowPriorityItem);
		system.Assembly.AddItem(assembly, 1, highPriorityItem);

		var resolved = system.Resolver.Resolve(assembly);

		TestAssert.False(resolved.Flags.Contains(flagId), "Higher-priority flag with value 0 should suppress the flag.");
	}

	private static void FlagsExcludeNonPositive()
	{
		var system = new ItemAssemblySystem();
		var assembly = system.AssemblyFactory.Create(1);
		var flagId = new ModifierId("disabled");

		var item = system.ItemFactory.Create(new[]
		{
			new Modifier(flagId, ModifierKind.Flag, 0f, Priority: 10)
		});

		system.Assembly.AddItem(assembly, 0, item);
		var resolved = system.Resolver.Resolve(assembly);

		TestAssert.False(resolved.Flags.Contains(flagId));
	}
}
