using Game.Systems.Domain.AgentBehaviour.Controller;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;

namespace Game.Systems.Domain.AgentBehaviour;

/// <summary>
/// Root orchestrator: wires behaviour registry, simulation, and output controllers.
/// </summary>
public sealed class AgentBehaviourSystem : IAgentBehaviourSystem
{
	public IBehaviourController Behaviour { get; }
	public IAgentBehaviourSimulation Simulation { get; }
	public IAgentBehaviourOutput Output { get; }

	public AgentBehaviourSystem(IBehaviourContextProvider contextProvider)
	{
		var store = new AgentBehaviourStateStore();

		Behaviour = new BehaviourController(store);
		Simulation = new AgentBehaviourSimulationController(store, contextProvider);
		Output = new AgentBehaviourOutputController(store);
	}
}
