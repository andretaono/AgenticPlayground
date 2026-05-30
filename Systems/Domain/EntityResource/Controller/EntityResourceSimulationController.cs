using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Domain.EntityResource.Controller;

internal sealed class EntityResourceSimulationController : IEntityResourceSimulation
{
	private readonly EntityResourceStateStore _store;

	public EntityResourceSimulationController(EntityResourceStateStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void AdvanceSimulation(float deltaTime)
	{
		if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
			throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

		foreach (var entityResources in _store.Resources.Values)
		{
			foreach (var resource in entityResources.Values)
				Step(resource, deltaTime);
		}
	}

	private static void Step(EntityResourceState resource, float deltaTime)
	{
		var netChange = (resource.RegenerationRate - resource.DepletionRate) * deltaTime;
		resource.CurrentAmount = Math.Clamp(resource.CurrentAmount + netChange, 0f, resource.MaximumAmount);
	}
}
