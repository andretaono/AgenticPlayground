using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Controller;

internal sealed class AgentBehaviourOutputController : IAgentBehaviourOutput
{
	private readonly AgentBehaviourStateStore _store;

	public AgentBehaviourOutputController(AgentBehaviourStateStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public IBehaviour? GetActiveBehaviour(AgentId agentId) =>
		_store.GetActiveBehaviour(agentId);

	public IReadOnlyList<IBehaviourIntent> GetEmittedIntents(AgentId agentId) =>
		_store.GetEmittedIntents(agentId);
}
