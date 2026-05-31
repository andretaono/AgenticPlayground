using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Integration.Combat;

public static class MeleeAttackAbilityFactory
{
	public static Ability Create(
		ICombatEntityRegistry registry,
		IEntityResourceRegistry resourceRegistry,
		float basePower = 25f) =>
		new(
			basePower,
			new ExplicitTargetTargetingRule(registry),
			Array.Empty<ICondition>(),
			new IEffect[] { new HealthDamageEffect(resourceRegistry) },
			Array.Empty<IEffectModifier>());
}
