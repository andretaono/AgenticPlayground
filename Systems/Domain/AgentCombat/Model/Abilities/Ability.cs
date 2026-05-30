using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Model;

public sealed class Ability
{
	public float BasePower { get; }
	public ITargetingRule Targeting { get; }
	public IReadOnlyList<ICondition> Conditions { get; }
	public IReadOnlyList<IEffect> Effects { get; }
	public IReadOnlyList<IEffectModifier> Modifiers { get; }

	public Ability(
		float basePower,
		ITargetingRule targeting,
		IEnumerable<ICondition> conditions,
		IEnumerable<IEffect> effects,
		IEnumerable<IEffectModifier> modifiers)
	{
		BasePower = basePower;
		Targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
		Conditions = (conditions ?? throw new ArgumentNullException(nameof(conditions))).ToList();
		Effects = (effects ?? throw new ArgumentNullException(nameof(effects))).ToList();
		Modifiers = (modifiers ?? throw new ArgumentNullException(nameof(modifiers))).ToList();
	}
}
