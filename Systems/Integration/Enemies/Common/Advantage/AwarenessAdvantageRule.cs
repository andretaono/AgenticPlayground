using Game.Systems.Domain.WorldCognition.Model;

namespace Game.Systems.Integration.Enemies.Common.Advantage;

public sealed class AwarenessAdvantageRule : IAttackAdvantageRule
{
	private readonly AwarenessState _minimumAwareness;

	public AwarenessAdvantageRule(AwarenessState minimumAwareness) => _minimumAwareness = minimumAwareness;

	public bool Evaluate(AdvantageContext context)
	{
		var awareness = context.Cognition.GetAwareness(context.AgentPosition);
		return awareness >= _minimumAwareness;
	}
}
