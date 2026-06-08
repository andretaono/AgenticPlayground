using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Tests.Integration.Runners;

public sealed class WorldTerrainMeshIntegrationRunner
{
	public WorldTerrainMeshIntegrationResult Run()
	{
		var dataSource = new InMemoryWorldDataSource(WorldIntegrationRunner.CreateDemoMap(width: 10, height: 6));
		var composer = new TerrainComposer(new DefaultTileRulesProvider());

		var mapping = new WorldTerrainMapping(
			Seed: 42,
			WorldUnitsPerTile: 1f,
			TerrainConfig: new TerrainMeshConfig { HeightScale = 1f },
			ModifierSettings: new TileHeightModifierSettings());

		var result = composer.Compose(dataSource, mapping);

		var waterX = 5;
		var waterY = 2;
		var waterHeight = result.Heightmap.Sample(waterX, waterY);

		var wallX = 0;
		var wallY = 0;
		var wallHeight = result.Heightmap.Sample(wallX, wallY);

		var groundX = 1;
		var groundY = 1;
		var groundHeight = result.Heightmap.Sample(groundX, groundY);

		return new WorldTerrainMeshIntegrationResult(
			WorldWidth: dataSource.Width,
			WorldHeight: dataSource.Height,
			HeightmapWidth: result.Heightmap.Width,
			HeightmapHeight: result.Heightmap.Height,
			WaterHeight: waterHeight,
			WallHeight: wallHeight,
			GroundHeight: groundHeight,
			WaterTileId: dataSource.LoadMap()[waterX, waterY].Id,
			WallTileId: dataSource.LoadMap()[wallX, wallY].Id,
			GroundTileId: dataSource.LoadMap()[groundX, groundY].Id);
	}
}

public sealed record WorldTerrainMeshIntegrationResult(
	int WorldWidth,
	int WorldHeight,
	int HeightmapWidth,
	int HeightmapHeight,
	float WaterHeight,
	float WallHeight,
	float GroundHeight,
	string WaterTileId,
	string WallTileId,
	string GroundTileId);
