using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

/// <summary>
/// Arc melee ability: targeting rules, conditions, and combat parameters live here—not on agents.
/// </summary>
public sealed class ArcAttackAbilityDefinition
{
	public static ArcAttackAbilityDefinition Default { get; } = new();

	public float BasePower { get; init; } = 20f;
	public float CooldownSeconds { get; init; } = 0.5f;
	public float ArcDegrees { get; init; } = 180f;
	public float Range { get; init; } = 2f;

	public Ability Build(
		ICombatEntityRegistry registry,
		IEntityResourceRegistry resourceRegistry,
		Func<EntityId, Vector2> getPosition,
		AgentOrientationStore orientation)
	{
		if (registry is null)
			throw new ArgumentNullException(nameof(registry));
		if (resourceRegistry is null)
			throw new ArgumentNullException(nameof(resourceRegistry));
		if (getPosition is null)
			throw new ArgumentNullException(nameof(getPosition));
		if (orientation is null)
			throw new ArgumentNullException(nameof(orientation));

		return new Ability(
			BasePower,
			new ArcTargetingRule(
				registry,
				getPosition,
				orientation,
				Range,
				ArcDegrees),
			new ICondition[] { new AliveTargetCondition(resourceRegistry) },
			new IEffect[] { new HealthDamageEffect(resourceRegistry) },
			Array.Empty<IEffectModifier>());
	}
}
