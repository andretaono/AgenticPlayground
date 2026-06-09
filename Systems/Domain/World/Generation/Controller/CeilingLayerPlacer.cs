using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal static class CeilingLayerPlacer
{
	public static CeilingPlacementResult Place(
		TileId[,] groundLayer,
		WorldPosition start,
		int seed,
		WorldGenerationConfig config,
		int[,] caveRegionIndex)
	{
		if (groundLayer is null)
			throw new ArgumentNullException(nameof(groundLayer));
		if (config is null)
			throw new ArgumentNullException(nameof(config));
		if (caveRegionIndex is null)
			throw new ArgumentNullException(nameof(caveRegionIndex));

		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);
		var ceilingLayer = CreateAirLayer(width, height);

		if (!config.EnableCeilingLayer)
			return new CeilingPlacementResult(ceilingLayer, caveRegionIndex);

		PlaceCaveCeilingsFromIndex(groundLayer, ceilingLayer, caveRegionIndex, start, config);
		PlaceClusteredExtraWallStacks(groundLayer, ceilingLayer, seed, config);

		return new CeilingPlacementResult(ceilingLayer, caveRegionIndex);
	}

	private static TileId[,] CreateAirLayer(int width, int height)
	{
		var layer = new TileId[width, height];

		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
			layer[x, y] = CeilingLayerTileIds.Air;

		return layer;
	}

	private static void PlaceCaveCeilingsFromIndex(
		TileId[,] groundLayer,
		TileId[,] ceilingLayer,
		int[,] caveRegionIndex,
		WorldPosition start,
		WorldGenerationConfig config)
	{
		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);
		var clearanceRadiusSquared = config.StartCeilingClearanceRadius * config.StartCeilingClearanceRadius;

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (caveRegionIndex[x, y] < 0)
					continue;

				if (groundLayer[x, y] != TileIds.Ground)
					continue;

				var dx = x - start.X;
				var dy = y - start.Y;
				if ((dx * dx) + (dy * dy) <= clearanceRadiusSquared)
					continue;

				ceilingLayer[x, y] = CeilingLayerTileIds.Solid;
			}
		}
	}

	/// <summary>
	/// Layer-4 stacks only on layer-3 wall cells. Seeds are sparse; growth passes expand along wall runs.
	/// </summary>
	private static void PlaceClusteredExtraWallStacks(
		TileId[,] groundLayer,
		TileId[,] ceilingLayer,
		int seed,
		WorldGenerationConfig config)
	{
		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);
		var stacked = new bool[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (!IsWall(groundLayer, x, y))
					continue;

				if (HasStackedWallNeighbor(stacked, groundLayer, x, y, width, height))
					continue;

				if (DeterministicCellRandom.Roll(seed, x, y, salt: 0x57414C) < config.ExtraWallStackChance)
					stacked[x, y] = true;
			}
		}

		for (var pass = 1; pass <= config.ExtraWallStackGrowPasses; pass++)
		{
			var grown = new bool[width, height];

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					if (stacked[x, y] || !IsWall(groundLayer, x, y))
						continue;

					if (!HasStackedWallNeighbor(stacked, groundLayer, x, y, width, height))
						continue;

					if (DeterministicCellRandom.Roll(seed, x, y, salt: 0x57414C + pass) <
					    config.ExtraWallStackClusterChance)
					{
						grown[x, y] = true;
					}
				}
			}

			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			{
				if (grown[x, y])
					stacked[x, y] = true;
			}
		}

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (!stacked[x, y] || !IsWall(groundLayer, x, y))
					continue;

				ceilingLayer[x, y] = CeilingLayerTileIds.Solid;
			}
		}
	}

	private static bool IsWall(TileId[,] groundLayer, int x, int y) =>
		groundLayer[x, y] == TileIds.Wall;

	private static bool HasStackedWallNeighbor(
		bool[,] stacked,
		TileId[,] groundLayer,
		int x,
		int y,
		int width,
		int height)
	{
		foreach (var neighbor in FloorTraversal.GetNeighbors(new WorldPosition(x, y)))
		{
			if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
				continue;

			if (!IsWall(groundLayer, neighbor.X, neighbor.Y))
				continue;

			if (stacked[neighbor.X, neighbor.Y])
				return true;
		}

		return false;
	}
}
