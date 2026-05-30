using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Scenarios;

public sealed class EntityResourceDemo : IScenario
{
	public string Name => "entity-resource";

	public void Run()
	{
		RunDepletionCase();
		Console.WriteLine();
		RunRegenerationCase();
		Console.WriteLine();
		RunCombinedCase();
	}

	private static void RunDepletionCase()
	{
		Console.WriteLine("=== Case 1: Stamina depletes to zero ===");

		var resources = new EntityResourceSystem();
		var entity = new EntityId(1);
		var staminaId = new ResourceId("stamina");

		resources.Registry.AddResource(entity, new ResourceDefinition(
			ResourceId: staminaId,
			Name: "Stamina",
			MaximumAmount: 50f,
			RegenerationRate: 0f,
			DepletionRate: 10f,
			InitialAmount: 50f));

		Console.WriteLine("Start:");
		PrintResource(resources, entity, staminaId);

		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = resources.Resource.GetResource(entity, staminaId);
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}");
			});

		Console.WriteLine($"Depleted: {resources.Resource.IsDepleted(entity, staminaId)}");
	}

	private static void RunRegenerationCase()
	{
		Console.WriteLine("=== Case 2: Mana regenerates to full ===");

		var resources = new EntityResourceSystem();
		var entity = new EntityId(2);
		var manaId = new ResourceId("mana");

		resources.Registry.AddResource(entity, new ResourceDefinition(
			ResourceId: manaId,
			Name: "Mana",
			MaximumAmount: 40f,
			RegenerationRate: 8f,
			DepletionRate: 0f,
			InitialAmount: 0f));

		Console.WriteLine("Start (empty):");
		PrintResource(resources, entity, manaId);

		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = resources.Resource.GetResource(entity, manaId);
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}");
			});

		Console.WriteLine($"Full: {resources.Resource.IsFull(entity, manaId)}");
	}

	private static void RunCombinedCase()
	{
		Console.WriteLine("=== Case 3: Hunger drains, then health regen after damage ===");

		var resources = new EntityResourceSystem();
		var entity = new EntityId(3);
		var hungerId = new ResourceId("hunger");
		var healthId = new ResourceId("health");

		resources.Registry.AddResource(entity, new ResourceDefinition(
			ResourceId: hungerId,
			Name: "Hunger",
			MaximumAmount: 100f,
			RegenerationRate: 0f,
			DepletionRate: 5f,
			InitialAmount: 25f));

		resources.Registry.AddResource(entity, new ResourceDefinition(
			ResourceId: healthId,
			Name: "Health",
			MaximumAmount: 100f,
			RegenerationRate: 10f,
			DepletionRate: 0f,
			InitialAmount: 100f));

		Console.WriteLine("Start:");
		PrintResource(resources, entity, hungerId);
		PrintResource(resources, entity, healthId);

		Console.WriteLine("\nSimulating hunger drain for 6 seconds...");
		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			snapshotIntervalSeconds: 1f,
			onSnapshot: (elapsed, _) =>
			{
				var hunger = resources.Resource.GetResource(entity, hungerId);
				Console.WriteLine($"t={elapsed:F1}s  {hunger.Name}={hunger.CurrentAmount:F1}  depleted={resources.Resource.IsDepleted(entity, hungerId)}");
			});

		Console.WriteLine("\nPlayer takes 60 damage...");
		resources.Resource.DecreaseResource(entity, healthId, 60f);
		PrintResource(resources, entity, healthId);

		Console.WriteLine("\nSimulating health regeneration for 4 seconds...");
		SimulateWithSnapshots(
			resources,
			seconds: 4f,
			tickRateHz: 20f,
			snapshotIntervalSeconds: 1f,
			onSnapshot: (elapsed, _) =>
			{
				var health = resources.Resource.GetResource(entity, healthId);
				Console.WriteLine($"t={elapsed:F1}s  {health.Name}={health.CurrentAmount:F1}/{health.MaximumAmount:F1}  full={resources.Resource.IsFull(entity, healthId)}");
			});
	}

	private static void SimulateWithSnapshots(
		EntityResourceSystem resources,
		float seconds,
		float tickRateHz,
		Action<float, int> onSnapshot,
		float snapshotIntervalSeconds = 0.5f)
	{
		var deltaTime = 1f / tickRateHz;
		var totalTicks = (int)(seconds * tickRateHz);
		var snapshotIntervalTicks = Math.Max(1, (int)(snapshotIntervalSeconds * tickRateHz));

		for (var tick = 1; tick <= totalTicks; tick++)
		{
			resources.Simulation.AdvanceSimulation(deltaTime);

			if (tick % snapshotIntervalTicks == 0)
				onSnapshot(tick * deltaTime, tick);
		}
	}

	private static void PrintResource(EntityResourceSystem resources, EntityId entityId, ResourceId resourceId)
	{
		var snapshot = resources.Resource.GetResource(entityId, resourceId);
		Console.WriteLine(
			$"- {snapshot.Name}: {snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1} " +
			$"(regen={snapshot.RegenerationRate}/s, depletion={snapshot.DepletionRate}/s)");
	}
}
