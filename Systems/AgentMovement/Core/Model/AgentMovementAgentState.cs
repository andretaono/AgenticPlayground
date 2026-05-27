using Game.AgentMovement.Interfaces;
using Game.Foundation.GameMath.Core.Model;

namespace Game.AgentMovement.Core.Model;

internal sealed class AgentMovementAgentState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 PendingInput;
    public AgentMovementState MovementState;
}

