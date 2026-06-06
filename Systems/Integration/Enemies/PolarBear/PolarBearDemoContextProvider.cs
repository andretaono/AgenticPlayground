using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Enemies.Common.Context;

namespace Game.Systems.Integration.Enemies.PolarBear;

public sealed class PolarBearDemoContextProvider : IBehaviourContextProvider
{
	private readonly TrackedTargetContextProvider _bearContext;
	private readonly AgentId _playerAgentId;
	private readonly Func<BehaviourContext> _playerContextFactory;

	public PolarBearDemoContextProvider(
		TrackedTargetContextProvider bearContext,
		AgentId playerAgentId,
		Func<BehaviourContext> playerContextFactory)
	{
		_bearContext = bearContext ?? throw new ArgumentNullException(nameof(bearContext));
		_playerAgentId = playerAgentId;
		_playerContextFactory = playerContextFactory ?? throw new ArgumentNullException(nameof(playerContextFactory));
	}

	public BehaviourContext GetContext(AgentId agentId) =>
		agentId.Equals(_playerAgentId)
			? _playerContextFactory()
			: _bearContext.GetContext(agentId);
}
