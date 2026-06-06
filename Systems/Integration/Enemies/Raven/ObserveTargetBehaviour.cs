using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Raven;

/// <summary>
/// Circles a tracked target at observe distance without committing to attack.
/// Validates non-predator wiring on shared perception and tactical config.
/// </summary>
public sealed class ObserveTargetBehaviour : IBehaviour
{
	private readonly ITargetTrackingState _tracking;
	private readonly EnemyTacticalConfig _config;
	private readonly float _observeDistance;

	public ObserveTargetBehaviour(
		ITargetTrackingState tracking,
		EnemyTacticalConfig config,
		float observeDistance)
	{
		_tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
		_config = config ?? throw new ArgumentNullException(nameof(config));
		_observeDistance = observeDistance;
	}

	public BehaviourId Id => new($"{_config.IdPrefix}-observe");
	public int Priority => _config.TrackPriority;

	public bool CanExecute(BehaviourContext context) => context.HasTarget;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context)
	{
		var target = _tracking.LastKnownTargetPosition;
		var delta = new Vector2(target.X - context.Position.X, target.Y - context.Position.Y);
		var distance = delta.Magnitude();
		if (distance <= 1e-6f)
			return Array.Empty<IBehaviourIntent>();

		var direction = delta.Normalized();
		if (distance < _observeDistance)
			direction = new Vector2(-direction.X, -direction.Y);

		return new IBehaviourIntent[]
		{
			new MoveBehaviourIntent(context.Agent, direction)
		};
	}
}
