using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Interfaces;

public interface IAgentMovementController
{
    AgentMovementState GetMovementState(EntityId entityId);
    void SetMovementState(EntityId entityId, AgentMovementState state);

    IVector3 GetPosition(EntityId entityId);
    IVector3 GetVelocity(EntityId entityId);

    void ApplyMovement(EntityId entityId, IVector3 input);
}

