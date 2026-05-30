using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Bridges EntityResource simulation port to the runtime tick schedule.
/// </summary>
public sealed class EntityResourceSimulationAdapter : ITickable
{
	private readonly IEntityResourceSimulation _simulation;

	public EntityResourceSimulationAdapter(IEntityResourceSimulation simulation)
	{
		_simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
	}

	public void Tick(float deltaTime) => _simulation.AdvanceSimulation(deltaTime);
}
