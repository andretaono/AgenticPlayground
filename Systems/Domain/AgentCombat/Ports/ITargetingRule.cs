using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface ITargetingRule
{
	IReadOnlyList<ICombatEntity> SelectTargets(AbilityContext context);
}
