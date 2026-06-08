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
		var baseHeightmap = _terrainMesh.Generator.Generate(mapping.Seed, width, height, terrainConfig);
		var modifiedSamples = TileHeightModifier.Apply(baseHeightmap, tiles, _tileRules, modifierSettings);
		var heightmap = Heightmap.FromSamples(modifiedSamples, mapping.WorldUnitsPerTile);
		var mesh = _terrainMesh.MeshBuilder.Build(heightmap, terrainConfig);

		return new WorldTerrainBuildResult(
			heightmap,
			mesh,
			FlattenTileOverlay(tiles, width, height));
	}

	private static TerrainMeshConfig WithCellSize(TerrainMeshConfig config, float cellSize) =>
		new()
		{
			CellSize = cellSize,
			HeightScale = config.HeightScale,
			MinHeight = config.MinHeight,
			MaxHeight = config.MaxHeight,
			NoiseFrequency = config.NoiseFrequency,
			NoiseOctaves = config.NoiseOctaves,
			NoisePersistence = config.NoisePersistence,
			NoiseLacunarity = config.NoiseLacunarity
		};

	private static IReadOnlyList<TileId> FlattenTileOverlay(TileId[,] tiles, int width, int height)
	{
		var overlay = new TileId[width * height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
				overlay[y * width + x] = tiles[x, y];
		}

		return overlay;
	}
}
