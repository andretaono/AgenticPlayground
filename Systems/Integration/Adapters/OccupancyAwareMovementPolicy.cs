using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;

namespace Game.Systems.Integration.Adapters;

public sealed class OccupancyAwareMovementPolicy : IAgentMovementPolicy
{
	private readonly IAgentMovementPolicy _inner;
	private readonly int _tileSize;
	private ITileOccupancyQuery? _occupancy;

	public OccupancyAwareMovementPolicy(IAgentMovementPolicy inner, int tileSize = 1)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		if (tileSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(tileSize));

		_tileSize = tileSize;
	}

	public void SetOccupancyQuery(ITileOccupancyQuery occupancy) =>
		_occupancy = occupancy ?? throw new ArgumentNullException(nameof(occupancy));

	public bool CanMoveTo(EntityId entityId, IVector3 proposedPosition, float bodyRadius)
	{
		if (!_inner.CanMoveTo(entityId, proposedPosition, bodyRadius))
			return false;

		if (_occupancy is null)
			return true;

		return MovementFootprint.CircleFits(
			proposedPosition.X,
			proposedPosition.Y,
			bodyRadius,
			_tileSize,
			(tileX, tileY) => _occupancy.IsTileOccupied(new WorldPosition(tileX, tileY), entityId));
	}
}
