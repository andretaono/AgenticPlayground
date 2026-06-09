using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementPolicy
{
	bool CanMoveTo(EntityId entityId, IVector3 proposedPosition);
}
