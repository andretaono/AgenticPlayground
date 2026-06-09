using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Allows all movement. Used by scenarios that do not model world collision.
/// </summary>
public sealed class PermissiveMovementPolicy : IAgentMovementPolicy
{
	public bool CanMoveTo(EntityId entityId, IVector3 proposedPosition) => true;
}
