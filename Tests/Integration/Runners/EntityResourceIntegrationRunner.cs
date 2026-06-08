using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Tests.Integration.Runners;

public sealed class EntityResourceIntegrationRunner
{
	public EntityResourceDepletionResult RunStaminaDepletion()
	{
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
		Simulate(resources, seconds: 6f, tickRateHz: 20f);

		return new EntityResourceDepletionResult(
			FinalAmount: stamina.CurrentAmount,
			IsDepleted: stamina.IsDepleted);
	}

	public EntityResourceRegenerationResult RunManaRegeneration()
	{
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
		Simulate(resources, seconds: 6f, tickRateHz: 20f);

		return new EntityResourceRegenerationResult(
			FinalAmount: mana.CurrentAmount,
			IsFull: mana.IsFull);
	}

	public EntityResourceCombinedResult RunHungerAndHealth()
	{
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

		Simulate(resources, seconds: 6f, tickRateHz: 20f);
		var hungerDepleted = hunger.IsDepleted;

		health.Decrease(60f);
		var healthAfterDamage = health.CurrentAmount;

		Simulate(resources, seconds: 4f, tickRateHz: 20f);

		return new EntityResourceCombinedResult(
			HungerDepleted: hungerDepleted,
			HealthAfterDamage: healthAfterDamage,
			FinalHealth: health.CurrentAmount,
			HealthRegenerated: health.CurrentAmount > healthAfterDamage);
	}

	private static void Simulate(EntityResourceSystem resources, float seconds, float tickRateHz)
	{
		var deltaTime = 1f / tickRateHz;
		var totalTicks = (int)(seconds * tickRateHz);

		for (var tick = 1; tick <= totalTicks; tick++)
			resources.Simulation.AdvanceSimulation(deltaTime);
	}
}

public sealed record EntityResourceDepletionResult(float FinalAmount, bool IsDepleted);

public sealed record EntityResourceRegenerationResult(float FinalAmount, bool IsFull);

public sealed record EntityResourceCombinedResult(
	bool HungerDepleted,
	float HealthAfterDamage,
	float FinalHealth,
	bool HealthRegenerated);
