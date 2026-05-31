using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class HealthDamageEffect : IEffect
{
	private readonly IEntityResourceRegistry _registry;

	public HealthDamageEffect(IEntityResourceRegistry registry) =>
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));

	public void Apply(EffectContext context) =>
		_registry.TryGetDefinition<IHealthResourceDefinition>(context.Target.EntityId)?.Decrease(context.Power);
}
