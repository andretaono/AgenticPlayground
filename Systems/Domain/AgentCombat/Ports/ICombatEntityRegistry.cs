using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface ICombatEntityRegistry
{
	void Register(ICombatEntity entity);
	void Unregister(ICombatEntity entity);
	bool TryGet(EntityId entityId, out ICombatEntity entity);
	IReadOnlyList<ICombatEntity> GetAllEntities();
}
