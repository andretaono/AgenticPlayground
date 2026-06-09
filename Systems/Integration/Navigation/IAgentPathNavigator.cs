using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Navigation;

public interface IAgentPathNavigator
{
	Vector2 GetMoveDirection(AgentId agentId, Vector2 from, Vector2 goalWorldPosition);
}
