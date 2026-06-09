using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class CombatFeedbackStore
{
	public sealed record AttackSwing(
		EntityId AttackerId,
		Vector2 Forward,
		float Range,
		float ArcDegrees,
		float OccurredAt);

	private readonly List<AttackSwing> _recentSwings = new();

	public void RecordSwing(
		EntityId attackerId,
		Vector2 forward,
		float range,
		float arcDegrees,
		float occurredAt) =>
		_recentSwings.Add(new AttackSwing(attackerId, forward, range, arcDegrees, occurredAt));

	public IReadOnlyList<AttackSwing> ConsumeRecentSwings()
	{
		if (_recentSwings.Count == 0)
			return Array.Empty<AttackSwing>();

		var copy = _recentSwings.ToArray();
		_recentSwings.Clear();
		return copy;
	}
}
