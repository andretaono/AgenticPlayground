namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementSystem
{
	IAgentMovementRegistry Registry { get; }
	IAgentMovementController Input { get; }
	IAgentMovementSimulation Simulation { get; }
}
