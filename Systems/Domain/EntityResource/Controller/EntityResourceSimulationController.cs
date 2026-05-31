using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Domain.EntityResource.Controller;

internal sealed class EntityResourceSimulationController : IEntityResourceSimulation
{
	private readonly EntityResourceRegistryController _registry;

	public EntityResourceSimulationController(EntityResourceRegistryController registry)
	{
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
	}

	public void AdvanceSimulation(float deltaTime)
	{
		if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
			throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

		foreach (var definition in _registry.AllDefinitions)
		{
			if (definition is ResourceDefinitionBase resource)
				resource.AdvanceSimulation(deltaTime);
		}
	}
}
