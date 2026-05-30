using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.Adapters
{
	public class AgentMovementPolicy : IAgentMovementPolicy
	{
		private readonly ITileRulesProvider _tileRulesProvider;
		private readonly InMemoryWorldDataSource _worldData;
		private readonly TileId[,] _map;

		public AgentMovementPolicy(
			ITileRulesProvider tileRulesProvider,
			InMemoryWorldDataSource worldData)
		{
			_tileRulesProvider = tileRulesProvider;
			_worldData = worldData;

			// Cache map for simplicity
			_map = _worldData.LoadMap();
		}

		public bool CanMove(AgentMovementAgentState agent)
		{
			// Predict target position
			var targetX = agent.Position.X + agent.Velocity.X;
			var targetY = agent.Position.Y + agent.Velocity.Y;

			// Convert world position -> tile coordinates
			var tileX = (int)(targetX / _worldData.TileSize);
			var tileY = (int)(targetY / _worldData.TileSize);

			// Bounds check
			if (tileX < 0 || tileY < 0 ||
				tileX >= _worldData.Width ||
				tileY >= _worldData.Height)
			{
				return false;
			}

			// Query tile
			var tileId = _map[tileX, tileY];

			// Query rules
			var rules = _tileRulesProvider.GetRules(tileId);

			// Block movement if tile blocks movement
			if (rules == TileRules.BlocksMovement)
			{
				return false;
			}

			return true;
		}
	}
}