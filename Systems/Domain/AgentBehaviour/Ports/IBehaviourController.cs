using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IBehaviourController
{
	void AddBehaviour(AgentId agentId, IBehaviour behaviour);
	bool RemoveBehaviour(AgentId agentId, IBehaviour behaviour);
	void SetBehaviourPriority(AgentId agentId, IBehaviour behaviour, int priority);
	void ClearBehaviours(AgentId agentId);
}
