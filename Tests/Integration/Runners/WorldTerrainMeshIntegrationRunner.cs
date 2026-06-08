using Game.Systems.Domain.TerrainMesh;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Tests.Integration.Runners;

public sealed class WorldTerrainMeshIntegrationRunner
{
	private const int BevelSegments = 4;

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
				HeightScale = 1f
			},
			ModifierSettings: new TileHeightModifierSettings
			{
				BevelInset = 0.3f,
				BevelSegments = BevelSegments
			});

		var result = composer.Compose(dataSource, mapping);

		var waterX = 5;
		var waterY = 2;
		var waterHeight = result.Heightmap.Sample(waterX, waterY);
		var waterVertexIndex = TileCenterVertexIndex(waterX, waterY, dataSource.Width);
		var waterVertexY = result.Mesh.Vertices[waterVertexIndex].Y;

		var wallX = 0;
		var wallY = 0;
		var wallHeight = result.Heightmap.Sample(wallX, wallY);
		var wallVertexIndex = TileCenterVertexIndex(wallX, wallY, dataSource.Width);
		var wallVertexY = result.Mesh.Vertices[wallVertexIndex].Y;

		var groundX = 1;
		var groundY = 1;
		var groundHeight = result.Heightmap.Sample(groundX, groundY);
		var groundVertexIndex = TileCenterVertexIndex(groundX, groundY, dataSource.Width);
		var groundVertexY = result.Mesh.Vertices[groundVertexIndex].Y;

		var groundNearWaterX = 3;
		var groundNearWaterY = 2;
		var groundEdgeVertexIndex = TileInsetEdgeVertexIndex(
			groundNearWaterX,
			groundNearWaterY,
			dataSource.Width,
			edgeX: 1,
			edgeZ: 0,
			insetSteps: 1);
		var groundEdgeVertexY = result.Mesh.Vertices[groundEdgeVertexIndex].Y;

		return new WorldTerrainMeshIntegrationResult(
			WorldWidth: dataSource.Width,
			WorldHeight: dataSource.Height,
			HeightmapWidth: result.Heightmap.Width,
			HeightmapHeight: result.Heightmap.Height,
			VertexCount: result.Mesh.Vertices.Count,
			TileOverlayCount: result.TileOverlay.Count,
			BevelSegments: BevelSegments,
			WaterHeight: waterHeight,
			WaterVertexY: waterVertexY,
			WallHeight: wallHeight,
			WallVertexY: wallVertexY,
			GroundHeight: groundHeight,
			GroundVertexY: groundVertexY,
			GroundEdgeNearWaterY: groundEdgeVertexY,
			WaterTileId: result.TileOverlay[waterVertexIndex].Id,
			WallTileId: result.TileOverlay[wallVertexIndex].Id,
			GroundTileId: result.TileOverlay[groundVertexIndex].Id);
	}

	internal static int TileCenterVertexIndex(int tileX, int tileY, int mapWidth, int segments = BevelSegments)
	{
		var gridWidth = mapWidth * segments + 1;
		var fx = tileX * segments + segments / 2;
		var fz = tileY * segments + segments / 2;
		return fz * gridWidth + fx;
	}

	internal static int TileInsetEdgeVertexIndex(
		int tileX,
		int tileY,
		int mapWidth,
		int edgeX,
		int edgeZ,
		int insetSteps,
		int segments = BevelSegments)
	{
		var gridWidth = mapWidth * segments + 1;
		var fx = edgeX < 0
			? tileX * segments + insetSteps
			: edgeX > 0
				? tileX * segments + segments - insetSteps
				: tileX * segments + segments / 2;
		var fz = edgeZ < 0
			? tileY * segments + insetSteps
			: edgeZ > 0
				? tileY * segments + segments - insetSteps
				: tileY * segments + segments / 2;
		return fz * gridWidth + fx;
	}
}

public sealed record WorldTerrainMeshIntegrationResult(
	int WorldWidth,
	int WorldHeight,
	int HeightmapWidth,
	int HeightmapHeight,
	int VertexCount,
	int TileOverlayCount,
	int BevelSegments,
	float WaterHeight,
	float WaterVertexY,
	float WallHeight,
	float WallVertexY,
	float GroundHeight,
	float GroundVertexY,
	float GroundEdgeNearWaterY,
	string WaterTileId,
	string WallTileId,
	string GroundTileId);
