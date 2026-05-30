using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IAgentBehaviourOutput
{
	IBehaviour? GetActiveBehaviour(AgentId agentId);
	IReadOnlyList<IBehaviourIntent> GetEmittedIntents(AgentId agentId);
}

public interface IAgentBehaviourSystem
{
	IBehaviourController Behaviour { get; }
	IAgentBehaviourSimulation Simulation { get; }
	IAgentBehaviourOutput Output { get; }
}
