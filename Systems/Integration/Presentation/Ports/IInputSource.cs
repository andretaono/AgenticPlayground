using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Presentation.Ports;

public interface IInputSource
{
	Vector2 PollMovementInput(AgentId agentId);
}
