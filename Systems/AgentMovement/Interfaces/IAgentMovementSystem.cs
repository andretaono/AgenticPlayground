using Game.Foundation.GameMath.Interfaces;
using Game.Foundation.Primitives;

namespace Game.AgentMovement.Interfaces;

public enum AgentMovementState
{
    Grounded,
    Swimming,
    Airborne
}

/// <summary>
/// Primary port for interacting with the agent movement use-case.
/// </summary>
public interface IAgentMovementSystem
{
    void CreateAgent(EntityId entityId, IVector3 initialPosition);
    bool RemoveAgent(EntityId entityId);

    AgentMovementState GetMovementState(EntityId entityId);
    void SetMovementState(EntityId entityId, AgentMovementState state);

    IVector3 GetPosition(EntityId entityId);
    IVector3 GetVelocity(EntityId entityId);

    void ApplyMovement(EntityId entityId, IVector3 input);

    // Port: advance system simulation time.
    void AdvanceSimulation(float deltaTime);
}

