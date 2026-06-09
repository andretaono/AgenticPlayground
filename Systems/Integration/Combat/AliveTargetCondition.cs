using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class AliveTargetCondition : ICondition
{
	private readonly IEntityResourceRegistry _resourceRegistry;

	public AliveTargetCondition(IEntityResourceRegistry resourceRegistry) =>
		_resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));

	public bool IsMet(AbilityContext context, ICombatEntity target)
	{
		_ = context;
		var health = _resourceRegistry.TryGetDefinition<IHealthResourceDefinition>(target.EntityId);
		if (health is null)
			return true;

		return !health.IsDepleted;
	}
}
