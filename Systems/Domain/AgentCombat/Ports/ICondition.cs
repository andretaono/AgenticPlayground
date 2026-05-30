using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface ICondition
{
	bool IsMet(AbilityContext context, ICombatEntity target);
}
