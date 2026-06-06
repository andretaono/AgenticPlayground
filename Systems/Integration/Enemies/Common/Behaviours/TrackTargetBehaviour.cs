using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Common.Behaviours;

public sealed class TrackTargetBehaviour : IBehaviour
{
	private readonly ITargetTrackingState _tracking;
	private readonly EnemyTacticalConfig _config;

	public TrackTargetBehaviour(ITargetTrackingState tracking, EnemyTacticalConfig config)
	{
		_tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
		_config = config ?? throw new ArgumentNullException(nameof(config));
	}

	public BehaviourId Id => new($"{_config.IdPrefix}-track");
	public int Priority => _config.TrackPriority;

	public bool CanExecute(BehaviourContext context)
	{
		if (!context.HasTarget)
			return false;

		return DistanceToTarget(context) > _config.StalkMaxDistance;
	}

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context) =>
		new IBehaviourIntent[]
		{
			new MoveBehaviourIntent(context.Agent, DirectionToTarget(context))
		};

	private float DistanceToTarget(BehaviourContext context) =>
		Distance(context.Position, _tracking.LastKnownTargetPosition);

	private Vector2 DirectionToTarget(BehaviourContext context)
	{
		var target = _tracking.LastKnownTargetPosition;
		var delta = new Vector2(target.X - context.Position.X, target.Y - context.Position.Y);
		return delta.Magnitude() <= 1e-6f ? Vector2.Zero : delta.Normalized();
	}

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}
}
