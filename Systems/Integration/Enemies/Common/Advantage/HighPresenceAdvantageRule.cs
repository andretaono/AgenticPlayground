namespace Game.Systems.Integration.Enemies.Common.Advantage;

public sealed class HighPresenceAdvantageRule : IAttackAdvantageRule
{
	private readonly float _threshold;

	public HighPresenceAdvantageRule(float threshold) => _threshold = threshold;

	public bool Evaluate(AdvantageContext context)
	{
		var targetCell = context.Cognition.GetCell(context.LastKnownTargetPosition);
		return targetCell.Presence >= _threshold;
	}
}
