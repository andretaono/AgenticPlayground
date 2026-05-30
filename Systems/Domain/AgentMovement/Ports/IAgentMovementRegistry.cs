using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementRegistry
{
	void CreateAgent(EntityId entityId, IVector3 initialPosition);
	bool RemoveAgent(EntityId entityId);
}
