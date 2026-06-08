using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Tests.Integration.Runners;

public sealed class ScriptedContextProvider : IBehaviourContextProvider
{
	private readonly AgentId _agentId;
	private readonly Func<BehaviourContext> _factory;

	public ScriptedContextProvider(AgentId agentId, Func<BehaviourContext> factory)
	{
		_agentId = agentId;
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
	}

	public BehaviourContext GetContext(AgentId agentId)
	{
		if (!agentId.Equals(_agentId))
			throw new KeyNotFoundException($"No scripted context for agent '{agentId}'.");

		return _factory();
	}
}
