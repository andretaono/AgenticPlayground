using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.TerrainMesh.Ports;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class WorldTerrainMeshComposer
{
	private readonly ITerrainMeshSystem _terrainMesh;
	private readonly ITileRulesProvider _tileRules;
	private readonly TileHeightModifierSettings _modifierSettings;

	public WorldTerrainMeshComposer(
		ITerrainMeshSystem terrainMesh,
		ITileRulesProvider tileRules,
		TileHeightModifierSettings? modifierSettings = null)
	{
		_terrainMesh = terrainMesh ?? throw new ArgumentNullException(nameof(terrainMesh));
		_tileRules = tileRules ?? throw new ArgumentNullException(nameof(tileRules));
		_modifierSettings = modifierSettings ?? new TileHeightModifierSettings();
	}

	public WorldTerrainBuildResult Compose(IWorldDataSource worldDataSource, WorldTerrainMapping mapping)
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

		var terrainConfig = WithCellSize(
			mapping.TerrainConfig ?? new TerrainMeshConfig(),
			mapping.WorldUnitsPerTile);

		var modifierSettings = mapping.ModifierSettings ?? _modifierSettings;
		var samples = TileHeightModifier.Build(tiles, _tileRules, modifierSettings);
		var heightmap = Heightmap.FromSamples(samples, mapping.WorldUnitsPerTile);
		var beveledMesh = BeveledTileMeshBuilder.Build(
			tiles,
			_tileRules,
			modifierSettings,
			terrainConfig.CellSize,
			terrainConfig.HeightScale);

		return new WorldTerrainBuildResult(
			heightmap,
			beveledMesh.Mesh,
			beveledMesh.VertexTileOverlay);
	}

	private static TerrainMeshConfig WithCellSize(TerrainMeshConfig config, float cellSize) =>
		new()
		{
			CellSize = cellSize,
			HeightScale = config.HeightScale,
			MinHeight = config.MinHeight,
			MaxHeight = config.MaxHeight
		};

}
