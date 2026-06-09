using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Actors;

public sealed class MovementTileOccupancyQuery : ITileOccupancyQuery
{
	private readonly IActorRegistry _actorRegistry;
	private readonly AgentMovementSystem _movement;
	private readonly WorldCoordinateConverter _coordinateConverter = new();
	private readonly int _tileSize;

	public MovementTileOccupancyQuery(
		IActorRegistry actorRegistry,
		AgentMovementSystem movement,
		int tileSize = 1)
	{
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
		if (tileSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(tileSize));

		_tileSize = tileSize;
	}

	public bool IsTileOccupied(WorldPosition tile, EntityId excludeEntityId)
	{
		foreach (var actor in _actorRegistry.Actors)
		{
			if (actor.EntityId.Equals(excludeEntityId))
				continue;

			var position = _movement.Input.GetPosition(actor.EntityId);
			var actorTile = _coordinateConverter.ToTilePosition(position.X, position.Y, _tileSize);
			if (actorTile == tile)
				return true;
		}

		return false;
	}
}
