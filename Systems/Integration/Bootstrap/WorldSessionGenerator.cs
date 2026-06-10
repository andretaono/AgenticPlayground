using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.TerrainMesh;
using Game.Systems.Integration.World;

namespace Game.Systems.Integration.Bootstrap;

public static class WorldSessionGenerator
{
	public static GeneratedWorldMap GenerateMap(GameSessionConfig config) =>
		new WorldGenerationSystem().Generator.Generate(config.World.Generation);

	public static TerrainBuildResult ComposeTerrain(GameSessionConfig config, GeneratedWorldMap map)
	{
		var terrain = config.Terrain;
		var composer = new TerrainComposer(new DefaultTileRulesProvider());
		return composer.ComposeFromMap(
			map,
			new WorldTerrainMapping(
				Seed: map.SeedUsed,
				WorldUnitsPerTile: terrain.WorldUnitsPerTile,
				TerrainConfig: new TerrainMeshConfig { HeightScale = terrain.HeightScale },
				ModifierSettings: terrain.Heights,
				SurfaceSettings: terrain.SurfaceMesh));
	}
}
