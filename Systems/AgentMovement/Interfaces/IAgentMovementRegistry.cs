using Game.Foundation.GameMath.Interfaces;
using Game.Foundation.Primitives;

namespace Game.AgentMovement.Interfaces;

public interface IAgentMovementRegistry
{
    void CreateAgent(EntityId entityId, IVector3 initialPosition);
    bool RemoveAgent(EntityId entityId);
}

