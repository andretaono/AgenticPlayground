using Game.Systems.Domain.Navigation.Model;
using Game.Systems.Domain.Navigation.Ports;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.Navigation.Controller;

public sealed class AStarGridPathfinder : IGridPathfinder
{
	public NavigationPath? TryFindPath(
		NavigationGrid grid,
		WorldPosition start,
		WorldPosition goal,
		Func<WorldPosition, bool>? isTileBlocked = null)
	{
		if (grid is null)
			throw new ArgumentNullException(nameof(grid));

		if (!IsTraversable(grid, start, isTileBlocked) || !IsTraversable(grid, goal, isTileBlocked))
			return null;

		if (start == goal)
			return new NavigationPath(new[] { start });

		var openSet = new List<WorldPosition> { start };
		var cameFrom = new Dictionary<WorldPosition, WorldPosition>();
		var gScore = new Dictionary<WorldPosition, int> { [start] = 0 };
		var fScore = new Dictionary<WorldPosition, int> { [start] = Heuristic(start, goal) };

		while (openSet.Count > 0)
		{
			var current = PopLowestFScore(openSet, fScore);
			if (current == goal)
				return new NavigationPath(ReconstructPath(cameFrom, current));

			openSet.Remove(current);

			foreach (var neighbor in GetNeighbors(current))
			{
				if (!IsTraversable(grid, neighbor, isTileBlocked))
					continue;

				var tentativeG = gScore[current] + grid.GetMoveCost(neighbor.X, neighbor.Y);
				if (gScore.TryGetValue(neighbor, out var existingG) && tentativeG >= existingG)
					continue;

				cameFrom[neighbor] = current;
				gScore[neighbor] = tentativeG;
				fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

				if (!openSet.Contains(neighbor))
					openSet.Add(neighbor);
			}
		}

		return null;
	}

	private static bool IsTraversable(
		NavigationGrid grid,
		WorldPosition tile,
		Func<WorldPosition, bool>? isTileBlocked)
	{
		if (!grid.IsWalkable(tile.X, tile.Y))
			return false;

		return isTileBlocked is null || !isTileBlocked(tile);
	}

	private static WorldPosition PopLowestFScore(List<WorldPosition> openSet, Dictionary<WorldPosition, int> fScore)
	{
		var bestIndex = 0;
		var bestScore = fScore[openSet[0]];

		for (var i = 1; i < openSet.Count; i++)
		{
			var score = fScore[openSet[i]];
			if (score < bestScore)
			{
				bestScore = score;
				bestIndex = i;
			}
		}

		return openSet[bestIndex];
	}

	private static IEnumerable<WorldPosition> GetNeighbors(WorldPosition position)
	{
		yield return new WorldPosition(position.X + 1, position.Y);
		yield return new WorldPosition(position.X - 1, position.Y);
		yield return new WorldPosition(position.X, position.Y + 1);
		yield return new WorldPosition(position.X, position.Y - 1);
	}

	private static int Heuristic(WorldPosition from, WorldPosition to) =>
		Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);

	private static IReadOnlyList<WorldPosition> ReconstructPath(
		Dictionary<WorldPosition, WorldPosition> cameFrom,
		WorldPosition current)
	{
		var path = new List<WorldPosition> { current };
		while (cameFrom.TryGetValue(current, out var previous))
		{
			current = previous;
			path.Add(current);
		}

		path.Reverse();
		return path;
	}
}
