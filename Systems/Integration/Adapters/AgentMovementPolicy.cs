using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Adapters;

public sealed class AgentMovementPolicy : IAgentMovementPolicy
{
	private readonly ITileRulesProvider _tileRulesProvider;
	private readonly InMemoryWorldDataSource _worldData;
	private readonly TileId[,] _map;

	public AgentMovementPolicy(
		ITileRulesProvider tileRulesProvider,
		InMemoryWorldDataSource worldData)
	{
		_tileRulesProvider = tileRulesProvider ?? throw new ArgumentNullException(nameof(tileRulesProvider));
		_worldData = worldData ?? throw new ArgumentNullException(nameof(worldData));
		_map = _worldData.LoadMap();
	}

	public bool CanMoveTo(EntityId entityId, IVector3 proposedPosition, float bodyRadius)
	{
		_ = entityId;
		if (bodyRadius < 0f)
			throw new ArgumentOutOfRangeException(nameof(bodyRadius), "Body radius must be non-negative.");

		return MovementFootprint.CircleFits(
			proposedPosition.X,
			proposedPosition.Y,
			bodyRadius,
			_worldData.TileSize,
			IsBlocked);
	}

	private bool IsBlocked(int tileX, int tileY)
	{
		if (tileX < 0 || tileY < 0 ||
			tileX >= _worldData.Width ||
			tileY >= _worldData.Height)
		{
			return true;
		}

		return _tileRulesProvider.GetRules(_map[tileX, tileY]).HasFlag(TileRules.BlocksMovement);
	}
}
