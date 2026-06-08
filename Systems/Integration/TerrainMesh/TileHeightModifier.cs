using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

internal static class TileHeightModifier
{
	public static float[,] Apply(
		Heightmap baseHeightmap,
		TileId[,] tiles,
		ITileRulesProvider rules,
		TileHeightModifierSettings settings)
	{
		if (baseHeightmap is null)
			throw new ArgumentNullException(nameof(baseHeightmap));
		if (tiles is null)
			throw new ArgumentNullException(nameof(tiles));
		if (rules is null)
			throw new ArgumentNullException(nameof(rules));
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));

		var width = baseHeightmap.Width;
		var height = baseHeightmap.Height;

		if (tiles.GetLength(0) != width || tiles.GetLength(1) != height)
			throw new ArgumentException("Tile grid dimensions must match the heightmap.");

		var result = new float[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var sample = baseHeightmap.Sample(x, y);
				var tileRules = rules.GetRules(tiles[x, y]);
				result[x, y] = ModifySample(sample, tileRules, settings);
			}
		}

		return result;
	}

	private static float ModifySample(float sample, TileRules tileRules, TileHeightModifierSettings settings)
	{
		if (tileRules.HasFlag(TileRules.BlocksMovement))
			return settings.CliffHeight;

		if (tileRules.HasFlag(TileRules.Swimable))
			return MathF.Min(sample, settings.SeaLevel);

		return sample;
	}
}
