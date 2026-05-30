using Game.Systems.Domain.AgentMovement.Model;

namespace Game.Systems.Domain.AgentMovement.Interfaces;

public interface IAgentMovementPolicy
{
	bool CanMove(AgentMovementAgentState agent);
}
