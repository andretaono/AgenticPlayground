using Game.Systems.Domain.EntityResource.Controller;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Domain.EntityResource;

/// <summary>
/// Root orchestrator: wires registry and simulation controllers.
/// </summary>
public sealed class EntityResourceSystem : IEntityResourceSystem
{
	public IEntityResourceRegistry Registry { get; }
	public IEntityResourceSimulation Simulation { get; }

	public EntityResourceSystem()
	{
		var registry = new EntityResourceRegistryController();

		Registry = registry;
		Simulation = new EntityResourceSimulationController(registry);
	}
}
