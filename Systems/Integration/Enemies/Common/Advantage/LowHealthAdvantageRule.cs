using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Integration.Enemies.Common.Advantage;

public sealed class LowHealthAdvantageRule : IAttackAdvantageRule
{
	private readonly float _threshold;

	public LowHealthAdvantageRule(float threshold) => _threshold = threshold;

	public bool Evaluate(AdvantageContext context)
	{
		var health = context.Resources.TryGetDefinition<IHealthResourceDefinition>(context.TargetEntity);
		return health is not null && health.CurrentAmount <= _threshold;
	}
}
