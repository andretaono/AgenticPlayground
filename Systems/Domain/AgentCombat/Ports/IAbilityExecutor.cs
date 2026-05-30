using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface IAbilityExecutor
{
	AbilityExecutionResult Execute(Ability ability, AbilityContext context);
}
