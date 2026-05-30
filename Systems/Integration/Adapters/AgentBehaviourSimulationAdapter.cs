using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Bridges AgentBehaviour simulation port to the runtime tick schedule.
/// </summary>
public sealed class AgentBehaviourSimulationAdapter : ITickable
{
	private readonly IAgentBehaviourSimulation _simulation;

	public AgentBehaviourSimulationAdapter(IAgentBehaviourSimulation simulation)
	{
		_simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
	}

	public void Tick(float deltaTime) => _simulation.Tick(deltaTime);
}
