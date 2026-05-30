namespace Game.Systems.Domain.AgentCombat.Ports;

public interface IAgentCombatSimulation
{
	void Tick(float deltaTime);
}
