using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Common.Context;

public sealed class TrackedTargetContextProvider : IBehaviourContextProvider
{
	private readonly AgentId _agentId;
	private readonly EntityId _entityId;
	private readonly EntityId _targetEntityId;
	private readonly Func<EntityId, Vector2> _getPosition;
	private readonly IWorldCognitionController _cognition;
	private readonly EcologicalTargetPerception _perception;
	private readonly PerceptionConfig _perceptionConfig;
	private readonly EnemyTacticalConfig _tacticalConfig;

	public EcologicalTargetPerception Perception => _perception;

	public TrackedTargetContextProvider(
		AgentId agentId,
		EntityId entityId,
		EntityId targetEntityId,
		Func<EntityId, Vector2> getPosition,
		IWorldCognitionController cognition,
		EcologicalTargetPerception perception,
		PerceptionConfig perceptionConfig,
		EnemyTacticalConfig tacticalConfig)
	{
		_agentId = agentId;
		_entityId = entityId;
		_targetEntityId = targetEntityId;
		_getPosition = getPosition ?? throw new ArgumentNullException(nameof(getPosition));
		_cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
		_perception = perception ?? throw new ArgumentNullException(nameof(perception));
		_perceptionConfig = perceptionConfig ?? throw new ArgumentNullException(nameof(perceptionConfig));
		_tacticalConfig = tacticalConfig ?? throw new ArgumentNullException(nameof(tacticalConfig));
	}

	public BehaviourContext GetContext(AgentId agentId)
	{
		if (!agentId.Equals(_agentId))
			throw new KeyNotFoundException($"No tracked-target context for agent '{agentId}'.");

		var agentPosition = _getPosition(_entityId);
		var targetPosition = _getPosition(_targetEntityId);

		_perception.Update(_cognition, agentPosition, targetPosition, _perceptionConfig);

		var delta = new Vector2(targetPosition.X - agentPosition.X, targetPosition.Y - agentPosition.Y);
		var distance = delta.Magnitude();
		var hasTarget = _perception.IsTracking;

		return new BehaviourContext
		{
			Agent = _agentId,
			Position = agentPosition,
			TargetEntity = hasTarget ? _targetEntityId : null,
			TargetDirection = distance <= 1e-6f ? Vector2.Zero : delta.Normalized(),
			TargetInAttackRange = hasTarget && distance <= _tacticalConfig.AttackRange
		};
	}
}
