using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

public sealed class AgentCombatSimulationAdapter : ITickable
{
	private readonly IAgentCombatSimulation _simulation;

	public AgentCombatSimulationAdapter(IAgentCombatSimulation simulation)
	{
		_simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
	}

	public void Tick(float deltaTime) => _simulation.Tick(deltaTime);
}
