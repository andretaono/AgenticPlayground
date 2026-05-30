using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Domain.AgentMovement.Interfaces;

public interface IAgentMovementPolicy
{
	bool CanMoveTo(IVector3 proposedPosition);
}
