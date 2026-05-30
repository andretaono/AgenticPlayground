namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementSimulation
{
	void AdvanceSimulation(float deltaTime);
}
