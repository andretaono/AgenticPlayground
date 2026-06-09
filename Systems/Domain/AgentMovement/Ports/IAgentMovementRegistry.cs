using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Ports;

public interface IAgentMovementRegistry
{
	void CreateAgent(EntityId entityId, IVector3 initialPosition, AgentMovementConfig? movementConfig = null);
	bool RemoveAgent(EntityId entityId);
}
