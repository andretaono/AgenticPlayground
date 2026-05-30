using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Allows all movement. Used by scenarios that do not model world collision.
/// </summary>
public sealed class PermissiveMovementPolicy : IAgentMovementPolicy
{
	public bool CanMoveTo(IVector3 proposedPosition) => true;
}
