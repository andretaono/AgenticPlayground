using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCombat.Controller;

public sealed class CombatEntityRegistryController : ICombatEntityRegistry
{
	private readonly List<ICombatEntity> _entities = new();

	public void Register(ICombatEntity entity)
	{
		if (entity is null)
			throw new ArgumentNullException(nameof(entity));

		if (!_entities.Contains(entity))
			_entities.Add(entity);
	}

	public void Unregister(ICombatEntity entity)
	{
		if (entity is null)
			throw new ArgumentNullException(nameof(entity));

		_entities.Remove(entity);
	}

	public bool TryGet(EntityId entityId, out ICombatEntity entity)
	{
		foreach (var candidate in _entities)
		{
			if (candidate.EntityId.Equals(entityId))
			{
				entity = candidate;
				return true;
			}
		}

		entity = null!;
		return false;
	}

	public IReadOnlyList<ICombatEntity> GetAllEntities() => _entities.AsReadOnly();
}
