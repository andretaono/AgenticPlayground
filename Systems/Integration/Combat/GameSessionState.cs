using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class GameSessionState
{
	private readonly HashSet<EntityId> _deadEntities = new();

	public bool PlayerIsDead { get; private set; }

	public bool IsDead(EntityId entityId) => _deadEntities.Contains(entityId);

	public void MarkPlayerDead() => PlayerIsDead = true;

	public void MarkEntityDead(EntityId entityId) => _deadEntities.Add(entityId);
}
