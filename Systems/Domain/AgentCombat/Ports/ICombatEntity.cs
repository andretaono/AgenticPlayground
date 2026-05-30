using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface ICombatEntity
{
	EntityId EntityId { get; }
	EntityId? PendingAttackTarget { get; set; }
	IReadOnlyList<IAbilityTrigger> AbilityTriggers { get; }
	void AddAbilityTrigger(IAbilityTrigger abilityTrigger);
}
