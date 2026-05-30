using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface IEffectModifier
{
	bool Applies(AbilityContext context, ICombatEntity target);
	float Modify(AbilityContext context, ICombatEntity target, float value);
}
