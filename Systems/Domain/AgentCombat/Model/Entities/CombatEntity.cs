using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCombat.Model;

public sealed class CombatEntity : ICombatEntity
{
	private readonly List<IAbilityTrigger> _abilityTriggers = new();

	public EntityId EntityId { get; }
	public EntityId? PendingAttackTarget { get; set; }
	public IReadOnlyList<IAbilityTrigger> AbilityTriggers => _abilityTriggers;

	public CombatEntity(EntityId entityId)
	{
		EntityId = entityId;
	}

	public void AddAbilityTrigger(IAbilityTrigger abilityTrigger)
	{
		_abilityTriggers.Add(abilityTrigger);
	}
}
