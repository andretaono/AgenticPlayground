using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Model;

internal sealed class AgentBehaviourEntry
{
	public required IBehaviour Behaviour { get; init; }
	public int? PriorityOverride { get; set; }

	public int EffectivePriority => PriorityOverride ?? Behaviour.Priority;
}

internal sealed class AgentBehaviourStateStore
{
	private readonly Dictionary<AgentId, List<AgentBehaviourEntry>> _behaviours = new();
	private readonly Dictionary<AgentId, IBehaviour?> _activeBehaviours = new();
	private readonly Dictionary<AgentId, List<IBehaviourIntent>> _emittedIntents = new();

	public IReadOnlyDictionary<AgentId, List<AgentBehaviourEntry>> Behaviours => _behaviours;

	public List<AgentBehaviourEntry> GetOrCreateBehaviours(AgentId agentId)
	{
		if (!_behaviours.TryGetValue(agentId, out var entries))
		{
			entries = new List<AgentBehaviourEntry>();
			_behaviours[agentId] = entries;
		}

		return entries;
	}

	public List<AgentBehaviourEntry> GetBehaviours(AgentId agentId) =>
		_behaviours.TryGetValue(agentId, out var entries) ? entries : new List<AgentBehaviourEntry>();

	public void SetActiveBehaviour(AgentId agentId, IBehaviour? behaviour) =>
		_activeBehaviours[agentId] = behaviour;

	public IBehaviour? GetActiveBehaviour(AgentId agentId) =>
		_activeBehaviours.TryGetValue(agentId, out var behaviour) ? behaviour : null;

	public void SetEmittedIntents(AgentId agentId, IReadOnlyList<IBehaviourIntent> intents) =>
		_emittedIntents[agentId] = intents.ToList();

	public IReadOnlyList<IBehaviourIntent> GetEmittedIntents(AgentId agentId) =>
		_emittedIntents.TryGetValue(agentId, out var intents) ? intents : Array.Empty<IBehaviourIntent>();

	public void ClearTickResults()
	{
		_activeBehaviours.Clear();
		_emittedIntents.Clear();
	}
}
