using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Model.Behaviours;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Controller;

internal sealed class AgentBehaviourSimulationController : IAgentBehaviourSimulation
{
	private readonly AgentBehaviourStateStore _store;
	private readonly IBehaviourContextProvider _contextProvider;
	private readonly IdleBehaviour _idleBehaviour = new();

	public AgentBehaviourSimulationController(
		AgentBehaviourStateStore store,
		IBehaviourContextProvider contextProvider)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
		_contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
	}

	public void Tick(float deltaTime)
	{
		if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
			throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

		_store.ClearTickResults();

		foreach (var (agentId, entries) in _store.Behaviours)
		{
			var context = _contextProvider.GetContext(agentId);
			var selected = SelectBehaviour(entries, context);
			var intents = selected.Execute(context);

			_store.SetActiveBehaviour(agentId, selected);
			_store.SetEmittedIntents(agentId, intents);
		}
	}

	private IBehaviour SelectBehaviour(IReadOnlyList<AgentBehaviourEntry> entries, BehaviourContext context)
	{
		AgentBehaviourEntry? best = null;

		foreach (var entry in entries)
		{
			if (!entry.Behaviour.CanExecute(context))
				continue;

			if (best is null || entry.EffectivePriority > best.EffectivePriority)
				best = entry;
		}

		return best?.Behaviour ?? _idleBehaviour;
	}
}
