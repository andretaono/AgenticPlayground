using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Controller;

public sealed class AbilityExecutor : IAbilityExecutor
{
	public AbilityExecutionResult Execute(Ability ability, AbilityContext context)
	{
		var executionState = new AbilityExecutionState();

		foreach (var target in ability.Targeting.SelectTargets(context))
		{
			if (!ConditionsAreMet(ability, context, target))
				continue;

			ApplyAbilityEffects(ability, context, target, executionState);
		}

		return executionState.ToResult();
	}

	private static void ApplyAbilityEffects(
		Ability ability,
		AbilityContext context,
		ICombatEntity target,
		AbilityExecutionState state)
	{
		foreach (var effect in ability.Effects)
		{
			var value = ApplyModifiers(ability, context, target);
			effect.Apply(new EffectContext(context, target, value));
			state.RegisterApplication(target, value);
		}
	}

	private static bool ConditionsAreMet(
		Ability ability,
		AbilityContext context,
		ICombatEntity target) =>
		ability.Conditions.All(c => c.IsMet(context, target));

	private static float ApplyModifiers(
		Ability ability,
		AbilityContext context,
		ICombatEntity target)
	{
		var value = ability.BasePower;

		foreach (var modifier in ability.Modifiers)
		{
			if (modifier.Applies(context, target))
				value = modifier.Modify(context, target, value);
		}

		return value;
	}
}
