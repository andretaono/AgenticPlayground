using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Tests.Fakes;

public sealed class FixedContextProvider : IBehaviourContextProvider
{
	private readonly Dictionary<AgentId, BehaviourContext> _contexts = new();

	public void Set(AgentId agentId, BehaviourContext context) =>
		_contexts[agentId] = context;

	public BehaviourContext GetContext(AgentId agentId) =>
		_contexts[agentId];
}
