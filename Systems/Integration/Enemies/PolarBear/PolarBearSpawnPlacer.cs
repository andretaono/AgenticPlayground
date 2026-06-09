using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Enemies.PolarBear;

public static class PolarBearSpawnPlacer
{
	private const int ShuffleSalt = 0xBEA0;

	public static IReadOnlyList<WorldPosition> Place(
		TileId[,] groundLayer,
		WorldPosition start,
		WorldPosition goal,
		int seed,
		int minCount,
		int maxCount)
	{
		if (groundLayer is null)
			throw new ArgumentNullException(nameof(groundLayer));

		var normalizedMin = Math.Max(0, Math.Min(minCount, maxCount));
		var normalizedMax = Math.Max(0, Math.Max(minCount, maxCount));
		if (normalizedMax == 0)
			return Array.Empty<WorldPosition>();

		var rng = new SeededRng(seed ^ ShuffleSalt);
		var targetCount = rng.Next(normalizedMin, normalizedMax + 1);

		var eligible = CollectEligibleGroundTiles(groundLayer, start, goal);
		if (eligible.Count == 0 || targetCount == 0)
			return Array.Empty<WorldPosition>();

		Shuffle(eligible, rng);

		var placedCount = Math.Min(targetCount, eligible.Count);
		return eligible.GetRange(0, placedCount);
	}

	private static List<WorldPosition> CollectEligibleGroundTiles(
		TileId[,] groundLayer,
		WorldPosition start,
		WorldPosition goal)
	{
		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);
		var eligible = new List<WorldPosition>();

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var position = new WorldPosition(x, y);
				if (IsAvailableGroundTile(groundLayer, position, start, goal))
					eligible.Add(position);
			}
		}

		return eligible;
	}

	private static bool IsAvailableGroundTile(
		TileId[,] groundLayer,
		WorldPosition position,
		WorldPosition start,
		WorldPosition goal)
	{
		if (position == start || position == goal)
			return false;

		return groundLayer[position.X, position.Y] == TileIds.Ground;
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
