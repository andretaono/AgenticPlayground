using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
namespace Game.Systems.Integration.Adapters;

public sealed class AgentMovementPolicy : IAgentMovementPolicy
{
	private readonly ITileRulesProvider _tileRulesProvider;
	private readonly InMemoryWorldDataSource _worldData;
	private readonly TileId[,] _map;

	private readonly WorldCoordinateConverter _coordinateConverter = new();

	public AgentMovementPolicy(
		ITileRulesProvider tileRulesProvider,
		InMemoryWorldDataSource worldData)
	{
		_tileRulesProvider = tileRulesProvider;
		_worldData = worldData;
		_map = _worldData.LoadMap();
	}

	public bool CanMoveTo(IVector3 proposedPosition)
	{
		var tile = _coordinateConverter.ToTilePosition(proposedPosition.X, proposedPosition.Y, _worldData.TileSize);
		var tileX = tile.X;
		var tileY = tile.Y;

		if (tileX < 0 || tileY < 0 ||
			tileX >= _worldData.Width ||
			tileY >= _worldData.Height)
		{
			return false;
		}

		var tileId = _map[tileX, tileY];
		var rules = _tileRulesProvider.GetRules(tileId);

		return rules != TileRules.BlocksMovement;
	}
}
