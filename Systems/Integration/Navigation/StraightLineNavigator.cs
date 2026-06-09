using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Navigation;

public sealed class StraightLineNavigator : IAgentPathNavigator
{
	public Vector2 GetMoveDirection(AgentId agentId, Vector2 from, Vector2 goalWorldPosition)
	{
		_ = agentId;
		var delta = new Vector2(goalWorldPosition.X - from.X, goalWorldPosition.Y - from.Y);
		return delta.Magnitude() <= 1e-6f ? Vector2.Zero : delta.Normalized();
	}
}
