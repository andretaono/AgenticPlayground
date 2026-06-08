using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class ItemAssemblyScenarioTests : ITestSuite
{
	public string Name => "item-assembly";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "sums flat modifiers across socketed items", () =>
		{
			var result = new ItemAssemblyIntegrationRunner().RunFlatAggregation();
			TestAssert.Equal(35f, result.Damage);
			TestAssert.Equal(1.5f, result.AttackRange);
			TestAssert.Equal(3, result.RawModifierCount);
		});

		registry.Add(Name, "resolves flat percent and flag modifiers", () =>
		{
			var result = new ItemAssemblyIntegrationRunner().RunMixedModifiers();
			TestAssert.Equal(2f, result.FlatMovementSpeed);
			TestAssert.Equal(0.15f, result.PercentMovementSpeed);
			TestAssert.True(result.CanSwimEnabled);
		});

		registry.Add(Name, "conflicting flags resolve by priority", () =>
		{
			var result = new ItemAssemblyIntegrationRunner().RunFlagPriority();
			TestAssert.True(result.CanFlyEnabled);
		});

		registry.Add(Name, "resolve output is deterministic", () =>
		{
			var result = new ItemAssemblyIntegrationRunner().RunDeterminism();
			TestAssert.True(result.Identical);
			TestAssert.True(result.FirstRawModifierCount > 0);
		});

		registry.Add(Name, "loot randomizer produces modifiers from catalog", () =>
		{
			var result = new ItemAssemblyIntegrationRunner().RunLootRandomizer();
			TestAssert.True(result.AllDropsHaveModifiers);
			TestAssert.True(result.DistinctModifierTypesRolled > 1);
			TestAssert.True(
				result.TopModifierId is "ground_health" or "beast_damage",
				$"Expected a high-weight modifier but was {result.TopModifierId}.");
			TestAssert.True(result.TopModifierRollCount > 100);
		});
	}
}
