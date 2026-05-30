using Game.Systems.Domain.AgentCombat.Model;

namespace Game.Systems.Domain.AgentCombat.Ports;

public interface IAbilityTrigger
{
	Ability Ability { get; }
	bool IsTriggered();
}
