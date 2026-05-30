using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface IEffect
{
	void Apply(EffectContext context);
}
