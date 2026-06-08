using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Tests.Fakes;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.EntityResource.Tests;

public sealed class EntityResourceTests : ITestSuite
{
	public string Name => "unit/entity-resource";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "decrease clamps at zero", DecreaseClampsAtZero);
		registry.Add(Name, "advance simulation regenerates health", AdvanceSimulationRegenerates);
	}

	private static void DecreaseClampsAtZero()
	{
		var system = new EntityResourceSystem();
		var entityId = new EntityId(1);
		var health = new FakeHealthResource(new ResourceId("health"), maximum: 100f, initialAmount: 30f);

		health.Attach(system.Registry, entityId);
		health.Decrease(50f);

		TestAssert.Equal(0f, health.CurrentAmount);
		TestAssert.True(health.IsDepleted);
	}

	private static void AdvanceSimulationRegenerates()
	{
		var system = new EntityResourceSystem();
		var entityId = new EntityId(2);
		var health = new FakeHealthResource(
			new ResourceId("health"),
			maximum: 100f,
			initialAmount: 50f,
			regenerationRate: 10f);

		health.Attach(system.Registry, entityId);
		system.Simulation.AdvanceSimulation(2f);

		TestAssert.Equal(70f, health.CurrentAmount);
	}
}
