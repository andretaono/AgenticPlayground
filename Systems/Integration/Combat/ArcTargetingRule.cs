using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class ArcTargetingRule : ITargetingRule
{
	private readonly ICombatEntityRegistry _registry;
	private readonly Func<EntityId, Vector2> _getPosition;
	private readonly AgentOrientationStore _orientation;
	private readonly float _range;
	private readonly float _halfArcCosine;

	public ArcTargetingRule(
		ICombatEntityRegistry registry,
		Func<EntityId, Vector2> getPosition,
		AgentOrientationStore orientation,
		float range,
		float arcDegrees)
	{
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		_getPosition = getPosition ?? throw new ArgumentNullException(nameof(getPosition));
		_orientation = orientation ?? throw new ArgumentNullException(nameof(orientation));
		if (range <= 0f)
			throw new ArgumentOutOfRangeException(nameof(range), "Range must be greater than zero.");

		_range = range;
		var halfArcRadians = arcDegrees * 0.5f * MathF.PI / 180f;
		_halfArcCosine = MathF.Cos(halfArcRadians);
	}

	public IReadOnlyList<ICombatEntity> SelectTargets(AbilityContext context)
	{
		var sourcePosition = _getPosition(context.Source.EntityId);
		var forward = _orientation.GetForward(context.Source.EntityId);
		var targets = new List<ICombatEntity>();

		foreach (var candidate in _registry.GetAllEntities())
		{
			if (candidate.EntityId.Equals(context.Source.EntityId))
				continue;

			if (!IsWithinArc(sourcePosition, forward, candidate.EntityId))
				continue;

			targets.Add(candidate);
		}

		return targets;
	}

	private bool IsWithinArc(Vector2 sourcePosition, Vector2 forward, EntityId targetId)
	{
		var offset = Subtract(_getPosition(targetId), sourcePosition);
		var distanceSquared = Dot(offset, offset);
		if (distanceSquared <= 1e-8f || distanceSquared > _range * _range)
			return false;

		var direction = Scale(offset, 1f / MathF.Sqrt(distanceSquared));
		return Dot(forward, direction) >= _halfArcCosine;
	}

	private static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

	private static Vector2 Subtract(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

	private static Vector2 Scale(Vector2 value, float scalar) => new(value.X * scalar, value.Y * scalar);
}
