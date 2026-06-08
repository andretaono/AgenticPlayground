using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

public static class GroundConnectivity
{
	public static bool HasGroundPath(TileId[,] tiles, WorldPosition start, WorldPosition goal)
	{
		if (tiles is null)
			throw new ArgumentNullException(nameof(tiles));

		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);

		if (!IsGround(tiles, start, width, height) || !IsGround(tiles, goal, width, height))
			return false;

		if (start == goal)
			return true;

		var visited = new bool[width, height];
		var queue = new Queue<WorldPosition>();
		queue.Enqueue(start);
		visited[start.X, start.Y] = true;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			if (current == goal)
				return true;

			foreach (var neighbor in GetNeighbors(current))
			{
				if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
					continue;

				if (visited[neighbor.X, neighbor.Y])
					continue;

				if (!IsGround(tiles, neighbor, width, height))
					continue;

				visited[neighbor.X, neighbor.Y] = true;
				queue.Enqueue(neighbor);
			}
		}

		return false;
	}

	public static bool TryPickStartAndGoal(
		TileId[,] tiles,
		out WorldPosition start,
		out WorldPosition goal)
	{
		start = default;
		goal = default;

		if (tiles is null)
			throw new ArgumentNullException(nameof(tiles));

		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);

		var hasStart = false;
		var hasGoal = false;
		var startScore = int.MaxValue;
		var goalScore = int.MinValue;

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var position = new WorldPosition(x, y);
				if (!IsGround(tiles, position, width, height))
					continue;

				var score = x + y;
				if (score < startScore)
				{
					startScore = score;
					start = position;
					hasStart = true;
				}

				if (score > goalScore)
				{
					goalScore = score;
					goal = position;
					hasGoal = true;
				}
			}
		}

		if (!hasStart || !hasGoal || start == goal)
			return false;

		return true;
	}

	private static bool IsGround(TileId[,] tiles, WorldPosition position, int width, int height)
	{
		if (position.X < 0 || position.Y < 0 || position.X >= width || position.Y >= height)
			return false;

		return tiles[position.X, position.Y] == TileIds.Ground;
	}

	private static IEnumerable<WorldPosition> GetNeighbors(WorldPosition position)
	{
		yield return new WorldPosition(position.X + 1, position.Y);
		yield return new WorldPosition(position.X - 1, position.Y);
		yield return new WorldPosition(position.X, position.Y + 1);
		yield return new WorldPosition(position.X, position.Y - 1);
	}
}
