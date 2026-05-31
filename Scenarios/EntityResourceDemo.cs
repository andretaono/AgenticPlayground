using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
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
		var stamina = new ResourceDefinition(
			resourceType: typeof(IStaminaResourceDefinition),
			resourceId: new ResourceId("stamina"),
			name: "Stamina",
			maximumAmount: 50f,
			regenerationRate: 0f,
			depletionRate: 10f,
			initialAmount: 50f);

		resources.Registry.AddResource(entity, stamina);

		Console.WriteLine("Start:");
		PrintResource(stamina);

		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = stamina.GetSnapshot();
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}");
			});

		Console.WriteLine($"Depleted: {stamina.IsDepleted}");
	}

	private static void RunRegenerationCase()
	{
		Console.WriteLine("=== Case 2: Mana regenerates to full ===");

		var resources = new EntityResourceSystem();
		var entity = new EntityId(2);
		var mana = new ResourceDefinition(
			resourceType: typeof(IManaResourceDefinition),
			resourceId: new ResourceId("mana"),
			name: "Mana",
			maximumAmount: 40f,
			regenerationRate: 8f,
			depletionRate: 0f,
			initialAmount: 0f);

		resources.Registry.AddResource(entity, mana);

		Console.WriteLine("Start (empty):");
		PrintResource(mana);

		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = mana.GetSnapshot();
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}");
			});

		Console.WriteLine($"Full: {mana.IsFull}");
	}

	private static void RunCombinedCase()
	{
		Console.WriteLine("=== Case 3: Hunger drains, then health regen after damage ===");

		var resources = new EntityResourceSystem();
		var entity = new EntityId(3);
		var hunger = new ResourceDefinition(
			resourceType: typeof(IHungerResourceDefinition),
			resourceId: new ResourceId("hunger"),
			name: "Hunger",
			maximumAmount: 100f,
			regenerationRate: 0f,
			depletionRate: 5f,
			initialAmount: 25f);
		var health = new ResourceDefinition(
			resourceType: typeof(IHealthResourceDefinition),
			resourceId: new ResourceId("health"),
			name: "Health",
			maximumAmount: 100f,
			regenerationRate: 10f,
			depletionRate: 0f,
			initialAmount: 100f);

		resources.Registry.AddResource(entity, hunger);
		resources.Registry.AddResource(entity, health);

		Console.WriteLine("Start:");
		PrintResource(hunger);
		PrintResource(health);

		Console.WriteLine("\nSimulating hunger drain for 6 seconds...");
		SimulateWithSnapshots(
			resources,
			seconds: 6f,
			tickRateHz: 20f,
			snapshotIntervalSeconds: 1f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = hunger.GetSnapshot();
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}  depleted={hunger.IsDepleted}");
			});

		Console.WriteLine("\nPlayer takes 60 damage...");
		health.Decrease(60f);
		PrintResource(health);

		Console.WriteLine("\nSimulating health regeneration for 4 seconds...");
		SimulateWithSnapshots(
			resources,
			seconds: 4f,
			tickRateHz: 20f,
			snapshotIntervalSeconds: 1f,
			onSnapshot: (elapsed, _) =>
			{
				var snapshot = health.GetSnapshot();
				Console.WriteLine($"t={elapsed:F1}s  {snapshot.Name}={snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}  full={health.IsFull}");
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

	private static void PrintResource(IResourceDefinition resource)
	{
		var snapshot = resource.GetSnapshot();
		Console.WriteLine(
			$"- {snapshot.Name}: {snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1} " +
			$"(regen={snapshot.RegenerationRate}/s, depletion={snapshot.DepletionRate}/s)");
	}
}
