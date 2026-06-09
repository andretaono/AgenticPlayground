using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Enemies.Common.Context;

public sealed class CompositeBehaviourContextProvider : IBehaviourContextProvider
{
	private readonly Dictionary<AgentId, IBehaviourContextProvider> _providers;

	public CompositeBehaviourContextProvider(IEnumerable<KeyValuePair<AgentId, IBehaviourContextProvider>> providers)
	{
		if (providers is null)
			throw new ArgumentNullException(nameof(providers));

		_providers = new Dictionary<AgentId, IBehaviourContextProvider>();
		foreach (var (agentId, provider) in providers)
		{
			if (provider is null)
				throw new ArgumentNullException(nameof(providers));

			_providers[agentId] = provider;
		}
	}

	public BehaviourContext GetContext(AgentId agentId)
	{
		if (!_providers.TryGetValue(agentId, out var provider))
			throw new KeyNotFoundException($"No behaviour context for agent '{agentId}'.");

		return provider.GetContext(agentId);
	}
}
