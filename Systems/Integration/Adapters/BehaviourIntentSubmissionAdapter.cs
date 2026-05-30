using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

public sealed class BehaviourIntentSubmissionAdapter : ITickable
{
	private readonly BehaviourIntentToCommandAdapter _intentAdapter;
	private readonly IReadOnlyList<AgentId> _agentIds;

	public BehaviourIntentSubmissionAdapter(
		BehaviourIntentToCommandAdapter intentAdapter,
		IReadOnlyList<AgentId> agentIds)
	{
		_intentAdapter = intentAdapter ?? throw new ArgumentNullException(nameof(intentAdapter));
		_agentIds = agentIds ?? throw new ArgumentNullException(nameof(agentIds));
	}

	public void Tick(float deltaTime)
	{
		foreach (var agentId in _agentIds)
			_intentAdapter.SubmitEmittedIntents(agentId);
	}
}
