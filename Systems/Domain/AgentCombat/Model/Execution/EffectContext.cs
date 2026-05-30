using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Model;

public sealed class EffectContext
{
	public AbilityContext AbilityContext { get; }
	public ICombatEntity Source => AbilityContext.Source;
	public ICombatEntity Target { get; }
	public float Power { get; }

	public EffectContext(AbilityContext abilityContext, ICombatEntity target, float power)
	{
		AbilityContext = abilityContext;
		Target = target;
		Power = power;
	}
}
