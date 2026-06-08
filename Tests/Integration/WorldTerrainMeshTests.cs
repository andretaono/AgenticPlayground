using Game.Systems.Foundation.Testing;
using Game.Tests.Integration.Runners;

namespace Game.Tests.Integration;

public sealed class WorldTerrainMeshTests : ITestSuite
{
	public string Name => "world-terrain-mesh";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "heightmap grid matches world dimensions", GridMatchesWorldDimensions);
		registry.Add(Name, "water tiles use water height", WaterTilesUseWaterHeight);
		registry.Add(Name, "wall tiles use wall height", WallTilesUseWallHeight);
		registry.Add(Name, "mesh vertex count matches beveled grid", MeshVertexCountMatchesBeveledGrid);
		registry.Add(Name, "tile center height matches heightmap sample", TileCenterHeightMatchesHeightmap);
		registry.Add(Name, "bevel softens height transitions", BevelSoftensHeightTransitions);
		registry.Add(Name, "tile overlay preserves world tile ids", TileOverlayPreservesTileIds);
	}

	private static void GridMatchesWorldDimensions()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(result.WorldWidth, result.HeightmapWidth);
		TestAssert.Equal(result.WorldHeight, result.HeightmapHeight);
	}

	private static void WaterTilesUseWaterHeight()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("water", result.WaterTileId);
		TestAssert.Equal(-1f, result.WaterHeight);
		TestAssert.Equal(-1f, result.WaterVertexY);
	}

	private static void WallTilesUseWallHeight()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("wall", result.WallTileId);
		TestAssert.Equal(1f, result.WallHeight);
		TestAssert.Equal(1f, result.WallVertexY);
	}

	private static void MeshVertexCountMatchesBeveledGrid()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();
		var gridWidth = result.WorldWidth * result.BevelSegments + 1;
		var gridHeight = result.WorldHeight * result.BevelSegments + 1;

		TestAssert.Equal(gridWidth * gridHeight, result.VertexCount);
		TestAssert.Equal(result.VertexCount, result.TileOverlayCount);
	}

	private static void TileCenterHeightMatchesHeightmap()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(result.GroundHeight, result.GroundVertexY);
		TestAssert.Equal(0f, result.GroundHeight);
	}

	private static void BevelSoftensHeightTransitions()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.True(result.GroundEdgeNearWaterY > result.WaterHeight);
		TestAssert.True(result.GroundEdgeNearWaterY < result.GroundHeight);
	}

	private static void TileOverlayPreservesTileIds()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("ground", result.GroundTileId);
	}
}
