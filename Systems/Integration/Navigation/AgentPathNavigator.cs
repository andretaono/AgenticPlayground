using Game.Systems.Domain.Navigation.Model;
using Game.Systems.Domain.Navigation.Ports;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;

namespace Game.Systems.Integration.Navigation;

public sealed class AgentPathNavigator : IAgentPathNavigator
{
	private const float WaypointReachRadius = 0.3f;
	private const float StuckMoveThreshold = 0.02f;
	private const int StuckTickThreshold = 5;

	private readonly NavigationGrid _grid;
	private readonly IGridPathfinder _pathfinder;
	private readonly ITileOccupancyQuery? _occupancy;
	private readonly WorldCoordinateConverter _coordinateConverter = new();
	private readonly int _tileSize;
	private readonly Dictionary<AgentId, AgentNavigationState> _states = new();

	public AgentPathNavigator(
		NavigationGrid grid,
		IGridPathfinder pathfinder,
		int tileSize = 1,
		ITileOccupancyQuery? occupancy = null)
	{
		_grid = grid ?? throw new ArgumentNullException(nameof(grid));
		_pathfinder = pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));
		if (tileSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(tileSize));

		_tileSize = tileSize;
		_occupancy = occupancy;
	}

	public Vector2 GetMoveDirection(AgentId agentId, Vector2 from, Vector2 goalWorldPosition)
	{
		var entityId = new EntityId(agentId.Value);
		var goalTile = _coordinateConverter.ToTilePosition(goalWorldPosition.X, goalWorldPosition.Y, _tileSize);
		if (!_states.TryGetValue(agentId, out var state))
		{
			state = new AgentNavigationState();
			_states[agentId] = state;
		}

		if (ShouldReplan(state, from, goalTile))
			Replan(state, from, goalTile, entityId);

		UpdateStuckTracking(state, from);

		if (state.Path is null || state.WaypointIndex >= state.Path.Waypoints.Count)
			return Vector2.Zero;

		var waypoint = TileCenter(state.Path.Waypoints[state.WaypointIndex]);
		var delta = new Vector2(waypoint.X - from.X, waypoint.Y - from.Y);
		var distance = delta.Magnitude();

		if (distance <= WaypointReachRadius)
		{
			state.WaypointIndex++;
			if (state.Path is null || state.WaypointIndex >= state.Path.Waypoints.Count)
				return Vector2.Zero;

			waypoint = TileCenter(state.Path.Waypoints[state.WaypointIndex]);
			delta = new Vector2(waypoint.X - from.X, waypoint.Y - from.Y);
			distance = delta.Magnitude();
		}

		if (distance <= 1e-6f)
			return Vector2.Zero;

		state.LastMoveDirection = delta.Normalized();
		return state.LastMoveDirection;
	}

	private bool ShouldReplan(AgentNavigationState state, Vector2 from, WorldPosition goalTile)
	{
		if (state.Path is null)
			return true;

		if (state.GoalTile != goalTile)
			return true;

		if (state.StuckTicks >= StuckTickThreshold)
			return true;

		if (state.WaypointIndex >= state.Path.Waypoints.Count)
			return true;

		var startTile = _coordinateConverter.ToTilePosition(from.X, from.Y, _tileSize);
		if (state.StartTile != startTile && state.WaypointIndex == 0)
			return true;

		return false;
	}

	private void Replan(AgentNavigationState state, Vector2 from, WorldPosition goalTile, EntityId entityId)
	{
		var startTile = _coordinateConverter.ToTilePosition(from.X, from.Y, _tileSize);
		if (!IsAvailableTile(startTile, entityId, allowOccupied: true))
		{
			var nearestStart = FindNearestAvailableTile(startTile, entityId, allowOccupied: true);
			if (nearestStart is null)
			{
				ClearPath(state);
				return;
			}

			startTile = nearestStart.Value;
		}

		if (!IsAvailableTile(goalTile, entityId))
		{
			var nearestGoal = FindNearestAvailableTile(goalTile, entityId);
			if (nearestGoal is null)
			{
				ClearPath(state);
				return;
			}

			goalTile = nearestGoal.Value;
		}

		bool IsBlocked(WorldPosition tile) =>
			tile != startTile && !IsAvailableTile(tile, entityId);

		var path = _pathfinder.TryFindPath(_grid, startTile, goalTile, IsBlocked);
		if (path is null)
		{
			ClearPath(state);
			return;
		}

		state.Path = path;
		state.StartTile = startTile;
		state.GoalTile = goalTile;
		state.WaypointIndex = path.Waypoints.Count > 1 ? 1 : 0;
		state.StuckTicks = 0;
		state.LastTrackedPosition = from;
	}

	private bool IsAvailableTile(WorldPosition tile, EntityId entityId, bool allowOccupied = false)
	{
		if (!_grid.IsWalkable(tile.X, tile.Y))
			return false;

		if (allowOccupied || _occupancy is null)
			return true;

		return !_occupancy.IsTileOccupied(tile, entityId);
	}

	private void UpdateStuckTracking(AgentNavigationState state, Vector2 from)
	{
		if (state.Path is null)
			return;

		var moved = Distance(from, state.LastTrackedPosition);
		if (moved >= StuckMoveThreshold)
		{
			state.StuckTicks = 0;
			state.LastTrackedPosition = from;
			return;
		}

		if (state.LastMoveDirection.Magnitude() > 1e-6f)
			state.StuckTicks++;
	}

	private WorldPosition? FindNearestAvailableTile(
		WorldPosition origin,
		EntityId entityId,
		bool allowOccupied = false)
	{
		if (IsAvailableTile(origin, entityId, allowOccupied))
			return origin;

		WorldPosition? best = null;
		var bestDistance = int.MaxValue;

		for (var y = 0; y < _grid.Height; y++)
		{
			for (var x = 0; x < _grid.Width; x++)
			{
				var candidate = new WorldPosition(x, y);
				if (!IsAvailableTile(candidate, entityId, allowOccupied))
					continue;

				var distance = Math.Abs(x - origin.X) + Math.Abs(y - origin.Y);
				if (distance >= bestDistance)
					continue;

				bestDistance = distance;
				best = candidate;
			}
		}

		return best;
	}

	private static void ClearPath(AgentNavigationState state)
	{
		state.Path = null;
		state.WaypointIndex = 0;
		state.LastMoveDirection = Vector2.Zero;
	}

	private static Vector2 TileCenter(WorldPosition tile) =>
		new(tile.X + 0.5f, tile.Y + 0.5f);

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	private sealed class AgentNavigationState
	{
		public NavigationPath? Path { get; set; }
		public int WaypointIndex { get; set; }
		public WorldPosition StartTile { get; set; }
		public WorldPosition GoalTile { get; set; }
		public Vector2 LastMoveDirection { get; set; }
		public Vector2 LastTrackedPosition { get; set; }
		public int StuckTicks { get; set; }
	}
}
