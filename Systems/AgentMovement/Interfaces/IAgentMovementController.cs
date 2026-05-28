using Game.AgentMovement.Model;
using Game.Foundation.GameMath.Interfaces;
using Game.Foundation.Primitives;

namespace Game.AgentMovement.Interfaces;

public interface IAgentMovementController
{
    AgentMovementState GetMovementState(EntityId entityId);
    void SetMovementState(EntityId entityId, AgentMovementState state);

    IVector3 GetPosition(EntityId entityId);
    IVector3 GetVelocity(EntityId entityId);

    void ApplyMovement(EntityId entityId, IVector3 input);
}

