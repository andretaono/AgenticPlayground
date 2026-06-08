using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal static class CaveCellularAutomata
{
	public static TileId[,] Generate(int seed, int width, int height, float fillProbability, int iterations)
	{
		if (width < 3)
			throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 3.");
		if (height < 3)
			throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 3.");
		if (fillProbability < 0f || fillProbability > 1f)
			throw new ArgumentOutOfRangeException(nameof(fillProbability), "Fill probability must be between 0 and 1.");
		if (iterations < 0)
			throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be non-negative.");

		var tiles = Initialize(seed, width, height, fillProbability);
		ForceBordersToWall(tiles, width, height);

		for (var i = 0; i < iterations; i++)
			tiles = Smooth(tiles, width, height);

		ForceBordersToWall(tiles, width, height);
		return tiles;
	}

	private static TileId[,] Initialize(int seed, int width, int height, float fillProbability)
	{
		var rng = new SeededRng(seed);
		var tiles = new TileId[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
				tiles[x, y] = rng.NextFloat() < fillProbability ? TileIds.Wall : TileIds.Ground;
		}

		return tiles;
	}

	private static TileId[,] Smooth(TileId[,] tiles, int width, int height)
	{
		var result = new TileId[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (IsBorder(x, y, width, height))
				{
					result[x, y] = TileIds.Wall;
					continue;
				}

				var wallNeighbors = CountWallNeighbors(tiles, x, y, width, height);
				result[x, y] = wallNeighbors >= 5 ? TileIds.Wall : TileIds.Ground;
			}
		}

		return result;
	}

	private static void ForceBordersToWall(TileId[,] tiles, int width, int height)
	{
		for (var x = 0; x < width; x++)
		{
			tiles[x, 0] = TileIds.Wall;
			tiles[x, height - 1] = TileIds.Wall;
		}

		for (var y = 0; y < height; y++)
		{
			tiles[0, y] = TileIds.Wall;
			tiles[width - 1, y] = TileIds.Wall;
		}
	}

	private static int CountWallNeighbors(TileId[,] tiles, int x, int y, int width, int height)
	{
		var count = 0;

		for (var dy = -1; dy <= 1; dy++)
		{
			for (var dx = -1; dx <= 1; dx++)
			{
				if (dx == 0 && dy == 0)
					continue;

				var nx = x + dx;
				var ny = y + dy;

				if (nx < 0 || ny < 0 || nx >= width || ny >= height)
				{
					count++;
					continue;
				}

				if (tiles[nx, ny] == TileIds.Wall)
					count++;
			}
		}

		return count;
	}

	private static bool IsBorder(int x, int y, int width, int height) =>
		x == 0 || y == 0 || x == width - 1 || y == height - 1;
}
