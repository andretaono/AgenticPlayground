using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Domain.AgentMovement.Model;

public enum AgentMovementState
{
	Grounded,
	Swimming,
	Airborne
}

public sealed class AgentMovementAgentState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 PendingInput;
    public AgentMovementState MovementState;
    public AgentMovementConfig MovementConfig = null!;
}

