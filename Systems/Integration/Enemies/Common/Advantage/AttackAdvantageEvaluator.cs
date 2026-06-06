namespace Game.Systems.Integration.Enemies.Common.Advantage;

/// <summary>
/// Advantage uses OR semantics: any one rule passing is enough to commit.
/// Low health is an opportunistic finish, not a prerequisite — resting prey
/// (high presence) or high ecosystem awareness also qualify.
/// </summary>
public sealed class AttackAdvantageEvaluator
{
	private readonly IReadOnlyList<IAttackAdvantageRule> _rules;

	public AttackAdvantageEvaluator(IReadOnlyList<IAttackAdvantageRule> rules)
	{
		_rules = rules ?? throw new ArgumentNullException(nameof(rules));
	}

	public bool HasAdvantage(AdvantageContext context) =>
		_rules.Any(rule => rule.Evaluate(context));
}
