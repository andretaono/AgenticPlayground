using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Ports;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class TerrainComposer
{
	private readonly ITileRulesProvider _tileRules;
	private readonly TileHeightModifierSettings _modifierSettings;

	public TerrainComposer(
		ITileRulesProvider tileRules,
		TileHeightModifierSettings? modifierSettings = null)
	{
		_tileRules = tileRules ?? throw new ArgumentNullException(nameof(tileRules));
		_modifierSettings = modifierSettings ?? new TileHeightModifierSettings();
	}

	public TerrainBuildResult Compose(IWorldDataSource worldDataSource, WorldTerrainMapping mapping)
	{
		if (worldDataSource is null)
			throw new ArgumentNullException(nameof(worldDataSource));
		if (mapping is null)
			throw new ArgumentNullException(nameof(mapping));
		if (mapping.WorldUnitsPerTile <= 0f)
			throw new ArgumentOutOfRangeException(nameof(mapping), "WorldUnitsPerTile must be greater than zero.");

		var tiles = worldDataSource.LoadMap();
		var width = worldDataSource.Width;
		var height = worldDataSource.Height;

		if (tiles.GetLength(0) != width || tiles.GetLength(1) != height)
			throw new InvalidOperationException("Loaded map dimensions do not match the data source.");

		var modifierSettings = mapping.ModifierSettings ?? _modifierSettings;
		var samples = TileHeightModifier.Build(tiles, _tileRules, modifierSettings);
		var heightmap = Heightmap.FromSamples(samples, mapping.WorldUnitsPerTile);

		return new TerrainBuildResult(heightmap);
	}

	public TerrainBuildResult ComposeFromMap(GeneratedWorldMap map, WorldTerrainMapping mapping)
	{
		var heightmapResult = Compose(map.ToDataSource(), mapping);
		var surfaceMesh = new TileSurfaceComposer(_tileRules).Compose(map, mapping);
		return heightmapResult with { SurfaceMesh = surfaceMesh };
	}
}
