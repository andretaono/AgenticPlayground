using Game.Systems.Domain.EntityResource.Controller;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Domain.EntityResource;

/// <summary>
/// Root orchestrator: wires registry, resource, and simulation controllers.
/// </summary>
public sealed class EntityResourceSystem : IEntityResourceSystem
{
	public IEntityResourceRegistry Registry { get; }
	public IEntityResourceController Resource { get; }
	public IEntityResourceSimulation Simulation { get; }

	public EntityResourceSystem()
	{
		var store = new EntityResourceStateStore();

		Registry = new EntityResourceRegistryController(store);
		Resource = new EntityResourceController(store);
		Simulation = new EntityResourceSimulationController(store);
	}
}
