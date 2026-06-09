using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;

namespace Game.Systems.Integration.Enemies.Common.Behaviours;

public sealed class StalkTargetBehaviour : IBehaviour
{
	private readonly ITargetTrackingState _tracking;
	private readonly EnemyTacticalConfig _config;
	private readonly IAgentPathNavigator _navigator;

	public StalkTargetBehaviour(
		ITargetTrackingState tracking,
		EnemyTacticalConfig config,
		IAgentPathNavigator navigator)
	{
		_tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
		_config = config ?? throw new ArgumentNullException(nameof(config));
		_navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
	}

	public BehaviourId Id => new($"{_config.IdPrefix}-stalk");
	public int Priority => _config.StalkPriority;

	public bool CanExecute(BehaviourContext context)
	{
		if (!context.HasTarget || context.TargetInAttackRange)
			return false;

		return DistanceToTarget(context) <= _config.StalkMaxDistance;
	}

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context)
	{
		var goal = StalkGoalPosition(context);
		var direction = _navigator.GetMoveDirection(context.Agent, context.Position, goal);
		if (direction.Magnitude() <= 1e-6f)
			return Array.Empty<IBehaviourIntent>();

		return new IBehaviourIntent[]
		{
			new MoveBehaviourIntent(context.Agent, direction)
		};
	}

	private Vector2 StalkGoalPosition(BehaviourContext context)
	{
		var target = _tracking.LastKnownTargetPosition;
		var delta = new Vector2(target.X - context.Position.X, target.Y - context.Position.Y);
		var distance = delta.Magnitude();
		if (distance <= 1e-6f)
			return target;

		var direction = delta.Normalized();
		if (distance < _config.StalkMinDistance)
		{
			direction = new Vector2(-direction.X, -direction.Y);
			return new Vector2(
				target.X + direction.X * _config.StalkMinDistance,
				target.Y + direction.Y * _config.StalkMinDistance);
		}

		return target;
	}

	private float DistanceToTarget(BehaviourContext context) =>
		Distance(context.Position, _tracking.LastKnownTargetPosition);

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}
}
