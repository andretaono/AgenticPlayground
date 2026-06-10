using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Controller;

internal sealed class BehaviourController : IBehaviourController
{
	private readonly AgentBehaviourStateStore _store;

	public BehaviourController(AgentBehaviourStateStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void AddBehaviour(AgentId agentId, IBehaviour behaviour)
	{
		if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));

		var entries = _store.GetOrCreateBehaviours(agentId);
		if (entries.Any(entry => entry.Behaviour.Id.Equals(behaviour.Id)))
			throw new InvalidOperationException($"Agent '{agentId}' already has behaviour '{behaviour.Id}'.");

		entries.Add(new AgentBehaviourEntry { Behaviour = behaviour });
	}

	public bool RemoveBehaviour(AgentId agentId, IBehaviour behaviour)
	{
		if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));

		var entries = _store.GetBehaviours(agentId);
		var index = entries.FindIndex(entry => ReferenceEquals(entry.Behaviour, behaviour));
		if (index < 0) return false;

		entries.RemoveAt(index);
		return true;
	}

	public void SetBehaviourPriority(AgentId agentId, IBehaviour behaviour, int priority)
	{
		if (behaviour is null) throw new ArgumentNullException(nameof(behaviour));

		var entry = _store.GetBehaviours(agentId)
			.FirstOrDefault(e => ReferenceEquals(e.Behaviour, behaviour));

		if (entry is null)
			throw new KeyNotFoundException($"Behaviour '{behaviour.Id}' is not assigned to agent '{agentId}'.");

		entry.PriorityOverride = priority;
	}

	public void ClearBehaviours(AgentId agentId) => _store.GetOrCreateBehaviours(agentId).Clear();

	public void UnregisterAgent(AgentId agentId) => _store.RemoveAgent(agentId);
}
