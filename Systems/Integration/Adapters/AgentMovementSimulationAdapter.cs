using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Bridges AgentMovement simulation port to the runtime tick schedule.
/// </summary>
public sealed class AgentMovementSimulationAdapter : ITickable
{
	private readonly IAgentMovementSimulation _simulation;

	public AgentMovementSimulationAdapter(IAgentMovementSimulation simulation)
	{
		_simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
	}

	public void Tick(float deltaTime) => _simulation.AdvanceSimulation(deltaTime);
}
