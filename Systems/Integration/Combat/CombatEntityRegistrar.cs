using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Resources;

namespace Game.Systems.Integration.Combat;

public static class CombatEntityRegistrar
{
	public static CombatEntity RegisterArcAttacker(
		AgentCombatSystem combat,
		EntityResourceSystem resources,
		CombatRuntimeServices services,
		EntityId entityId,
		ArcAttackAbilityDefinition abilityDefinition,
		float maxHealth,
		Func<EntityId, Vector2> getPosition)
	{
		if (combat is null)
			throw new ArgumentNullException(nameof(combat));
		if (resources is null)
			throw new ArgumentNullException(nameof(resources));
		if (services is null)
			throw new ArgumentNullException(nameof(services));
		if (abilityDefinition is null)
			throw new ArgumentNullException(nameof(abilityDefinition));
		if (getPosition is null)
			throw new ArgumentNullException(nameof(getPosition));

		if (!HasHealth(resources, entityId))
			AttachHealth(resources, entityId, maxHealth);

		var ability = abilityDefinition.Build(
			combat.Registry,
			resources.Registry,
			getPosition,
			services.Orientation);

		var combatEntity = new CombatEntity(entityId);
		combatEntity.AddAbilityTrigger(new CooldownAbilityTrigger(
			abilityDefinition,
			ability,
			combatEntity,
			services.CooldownTracker,
			() => services.CurrentTime));

		combat.Registry.Register(combatEntity);
		services.RegisterAbilityDefinition(ability, abilityDefinition);
		return combatEntity;
	}

	private static bool HasHealth(EntityResourceSystem resources, EntityId entityId) =>
		resources.Registry.TryGetDefinition<IHealthResourceDefinition>(entityId) is not null;

	private static void AttachHealth(EntityResourceSystem resources, EntityId entityId, float maximum)
	{
		var health = new HealthResource(entityId, maximum);
		health.Attach(resources.Registry, entityId);
	}
}
