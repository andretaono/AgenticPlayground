using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal static class WaterPlacer
{
	public static void Apply(
		TileId[,] tiles,
		WorldPosition start,
		WorldPosition goal,
		int seed,
		WorldGenerationConfig config)
	{
		if (tiles is null)
			throw new ArgumentNullException(nameof(tiles));
		if (config is null)
			throw new ArgumentNullException(nameof(config));
		if (config.WaterPoolAttempts < 1 || config.WaterPoolMaxSize < 1)
			return;

		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);
		var eligible = new List<WorldPosition>();

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var position = new WorldPosition(x, y);
				if (IsEligible(tiles, position, start, goal))
					eligible.Add(position);
			}
		}

		if (eligible.Count == 0)
			return;

		Shuffle(eligible, new SeededRng(seed ^ 0x5A7E));

		var poolsPlaced = 0;
		for (var i = 0; i < eligible.Count && poolsPlaced < config.WaterPoolAttempts; i++)
		{
			if (!IsEligible(tiles, eligible[i], start, goal))
				continue;

			if (TryGrowPool(tiles, start, goal, eligible[i], config.WaterPoolMaxSize))
				poolsPlaced++;
		}
	}

	private static bool TryGrowPool(
		TileId[,] tiles,
		WorldPosition start,
		WorldPosition goal,
		WorldPosition seedPosition,
		int maxPoolSize)
	{
		if (!IsEligible(tiles, seedPosition, start, goal))
			return false;

		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);
		var queue = new Queue<WorldPosition>();
		var visited = new HashSet<WorldPosition>();
		queue.Enqueue(seedPosition);
		visited.Add(seedPosition);

		var placed = 0;

		while (queue.Count > 0 && placed < maxPoolSize)
		{
			var position = queue.Dequeue();
			if (!IsEligible(tiles, position, start, goal))
				continue;

			var previousTile = tiles[position.X, position.Y];
			tiles[position.X, position.Y] = TileIds.Water;

			if (!GroundConnectivity.HasGroundPath(tiles, start, goal))
			{
				tiles[position.X, position.Y] = previousTile;
				continue;
			}

			placed++;

			foreach (var neighbor in GetNeighbors(position))
			{
				if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
					continue;

				if (!visited.Add(neighbor))
					continue;

				if (tiles[neighbor.X, neighbor.Y] == TileIds.Ground)
					queue.Enqueue(neighbor);
			}
		}

		return placed > 0;
	}

	private static bool IsEligible(TileId[,] tiles, WorldPosition position, WorldPosition start, WorldPosition goal)
	{
		if (position == start || position == goal)
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

	private static void Shuffle(List<WorldPosition> positions, SeededRng rng)
	{
		for (var i = positions.Count - 1; i > 0; i--)
		{
			var swapIndex = rng.Next(0, i + 1);
			(positions[i], positions[swapIndex]) = (positions[swapIndex], positions[i]);
		}
	}
}
