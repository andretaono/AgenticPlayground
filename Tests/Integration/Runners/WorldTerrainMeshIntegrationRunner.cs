using Game.Systems.Domain.TerrainMesh;
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
		var terrainMesh = new TerrainMeshSystem();
		var composer = new WorldTerrainMeshComposer(terrainMesh, new DefaultTileRulesProvider());

		var mapping = new WorldTerrainMapping(
			Seed: 42,
			WorldUnitsPerTile: 1f,
			TerrainConfig: new TerrainMeshConfig
			{
				MinHeight = 0f,
				MaxHeight = 10f,
				HeightScale = 1f,
				NoiseFrequency = 0.1f,
				NoiseOctaves = 2
			},
			ModifierSettings: new TileHeightModifierSettings
			{
				SeaLevel = 0.5f,
				CliffHeight = 8f
			});

		var result = composer.Compose(dataSource, mapping);

		var waterX = 5;
		var waterY = 2;
		var waterHeight = result.Heightmap.Sample(waterX, waterY);
		var waterVertexIndex = waterY * result.Heightmap.Width + waterX;
		var waterVertexY = result.Mesh.Vertices[waterVertexIndex].Y;

		var wallX = 0;
		var wallY = 0;
		var wallHeight = result.Heightmap.Sample(wallX, wallY);
		var wallVertexIndex = wallY * result.Heightmap.Width + wallX;
		var wallVertexY = result.Mesh.Vertices[wallVertexIndex].Y;

		var groundX = 1;
		var groundY = 1;
		var groundHeight = result.Heightmap.Sample(groundX, groundY);
		var groundVertexIndex = groundY * result.Heightmap.Width + groundX;
		var groundVertexY = result.Mesh.Vertices[groundVertexIndex].Y;

		return new WorldTerrainMeshIntegrationResult(
			WorldWidth: dataSource.Width,
			WorldHeight: dataSource.Height,
			HeightmapWidth: result.Heightmap.Width,
			HeightmapHeight: result.Heightmap.Height,
			VertexCount: result.Mesh.Vertices.Count,
			TileOverlayCount: result.TileOverlay.Count,
			WaterHeight: waterHeight,
			WaterVertexY: waterVertexY,
			WallHeight: wallHeight,
			WallVertexY: wallVertexY,
			GroundHeight: groundHeight,
			GroundVertexY: groundVertexY,
			WaterTileId: result.TileOverlay[waterVertexIndex].Id,
			WallTileId: result.TileOverlay[wallVertexIndex].Id,
			GroundTileId: result.TileOverlay[groundVertexIndex].Id);
	}
}

public sealed record WorldTerrainMeshIntegrationResult(
	int WorldWidth,
	int WorldHeight,
	int HeightmapWidth,
	int HeightmapHeight,
	int VertexCount,
	int TileOverlayCount,
	float WaterHeight,
	float WaterVertexY,
	float WallHeight,
	float WallVertexY,
	float GroundHeight,
	float GroundVertexY,
	string WaterTileId,
	string WallTileId,
	string GroundTileId);
