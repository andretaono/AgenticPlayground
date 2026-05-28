namespace Game.Systems.Domain.AgentMovement.Interfaces;

public interface IAgentMovementSystem
{
    IAgentMovementRegistry Registry { get; }
    IAgentMovementController Input { get; }
    IAgentMovementSimulation Simulation { get; }
}

