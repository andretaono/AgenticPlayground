using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class AttackCooldownTracker
{
	private readonly Dictionary<EntityId, float> _lastUsedAt = new();

	public bool IsReady(EntityId entityId, float cooldownSeconds, float currentTime)
	{
		if (cooldownSeconds <= 0f)
			return true;

		if (!_lastUsedAt.TryGetValue(entityId, out var lastUsedAt))
			return true;

		return currentTime - lastUsedAt >= cooldownSeconds;
	}

	public void MarkUsed(EntityId entityId, float currentTime) =>
		_lastUsedAt[entityId] = currentTime;
}
