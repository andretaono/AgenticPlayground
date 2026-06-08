using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

internal static class TileHeightModifier
{
	public static float[,] Build(
		TileId[,] tiles,
		ITileRulesProvider rules,
		TileHeightModifierSettings settings)
	{
		if (tiles is null)
			throw new ArgumentNullException(nameof(tiles));
		if (rules is null)
			throw new ArgumentNullException(nameof(rules));
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));

		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);
		var result = new float[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var tileRules = rules.GetRules(tiles[x, y]);
				result[x, y] = HeightForTile(tileRules, settings);
			}
		}

		return result;
	}

	internal static float HeightForTile(TileRules tileRules, TileHeightModifierSettings settings)
	{
		if (tileRules.HasFlag(TileRules.BlocksMovement))
			return settings.WallHeight;

		if (tileRules.HasFlag(TileRules.Swimable))
			return settings.WaterHeight;

		return settings.GroundHeight;
	}
}
