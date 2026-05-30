using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class ExplicitTargetTargetingRule : ITargetingRule
{
	private readonly ICombatEntityRegistry _registry;

	public ExplicitTargetTargetingRule(ICombatEntityRegistry registry)
	{
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
	}

	public IReadOnlyList<ICombatEntity> SelectTargets(AbilityContext context)
	{
		var pendingTarget = context.Source.PendingAttackTarget;
		if (!pendingTarget.HasValue)
			return Array.Empty<ICombatEntity>();

		if (!_registry.TryGet(pendingTarget.Value, out var target))
			return Array.Empty<ICombatEntity>();

		return new[] { target };
	}
}
