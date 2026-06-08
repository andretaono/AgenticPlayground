using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

public sealed class AgentMovementStateAdapter : ITickable
{
	private readonly IActorRegistry _actorRegistry;
	private readonly AgentMovementSystem _movement;
	private readonly ITileRulesProvider _tileRulesProvider;
	private readonly InMemoryWorldDataSource _worldData;
	private readonly TileId[,] _map;
	private readonly WorldCoordinateConverter _coordinateConverter = new();

	public AgentMovementStateAdapter(
		IActorRegistry actorRegistry,
		AgentMovementSystem movement,
		ITileRulesProvider tileRulesProvider,
		InMemoryWorldDataSource worldData)
	{
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
		_tileRulesProvider = tileRulesProvider ?? throw new ArgumentNullException(nameof(tileRulesProvider));
		_worldData = worldData ?? throw new ArgumentNullException(nameof(worldData));
		_map = _worldData.LoadMap();
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		foreach (var actor in _actorRegistry.Actors)
		{
			var position = _movement.Input.GetPosition(actor.EntityId);
			var tile = _coordinateConverter.ToTilePosition(position.X, position.Y, _worldData.TileSize);

			if (tile.X < 0 || tile.Y < 0 ||
			    tile.X >= _worldData.Width ||
			    tile.Y >= _worldData.Height)
			{
				continue;
			}

			var rules = _tileRulesProvider.GetRules(_map[tile.X, tile.Y]);
			var state = rules.HasFlag(TileRules.Swimable)
				? AgentMovementState.Swimming
				: AgentMovementState.Grounded;

			_movement.Input.SetMovementState(actor.EntityId, state);
		}
	}
}
