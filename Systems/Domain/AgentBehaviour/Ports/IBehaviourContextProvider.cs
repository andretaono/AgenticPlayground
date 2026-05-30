using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IBehaviourContextProvider
{
	BehaviourContext GetContext(AgentId agentId);
}
