using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Controller;

public sealed class AbilityOrchestrator
{
	private readonly IAbilityExecutor _abilityExecutor;
	private readonly ICombatEntityRegistry _combatEntityRegistry;

	public AbilityOrchestrator(
		IAbilityExecutor abilityExecutor,
		ICombatEntityRegistry combatEntityRegistry)
	{
		_abilityExecutor = abilityExecutor ?? throw new ArgumentNullException(nameof(abilityExecutor));
		_combatEntityRegistry = combatEntityRegistry ?? throw new ArgumentNullException(nameof(combatEntityRegistry));
	}

	public void Update()
	{
		foreach (var entity in _combatEntityRegistry.GetAllEntities())
		{
			foreach (var trigger in entity.AbilityTriggers)
			{
				if (!trigger.IsTriggered())
					continue;

				_abilityExecutor.Execute(trigger.Ability, new AbilityContext(entity));
			}
		}
	}
}
