using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementPolicy
{
	bool CanMoveTo(IVector3 proposedPosition);
}
