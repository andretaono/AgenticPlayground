using Game.Systems.Foundation.Testing;
using Game.Tests.Integration.Runners;

namespace Game.Tests.Integration;

public sealed class WorldTerrainMeshTests : ITestSuite
{
	public string Name => "world-terrain-mesh";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "heightmap grid matches world dimensions", GridMatchesWorldDimensions);
		registry.Add(Name, "water tiles clamp to sea level", WaterTilesClampToSeaLevel);
		registry.Add(Name, "wall tiles raise to cliff height", WallTilesRaiseToCliffHeight);
		registry.Add(Name, "mesh vertex count matches tile grid", MeshVertexCountMatchesGrid);
		registry.Add(Name, "height sample matches mesh vertex at same cell", HeightSampleMatchesMeshVertex);
		registry.Add(Name, "tile overlay preserves world tile ids", TileOverlayPreservesTileIds);
	}

	private static void GridMatchesWorldDimensions()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(result.WorldWidth, result.HeightmapWidth);
		TestAssert.Equal(result.WorldHeight, result.HeightmapHeight);
	}

	private static void WaterTilesClampToSeaLevel()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("water", result.WaterTileId);
		TestAssert.True(result.WaterHeight <= 0.5f);
		TestAssert.Equal(result.WaterHeight, result.WaterVertexY);
	}

	private static void WallTilesRaiseToCliffHeight()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("wall", result.WallTileId);
		TestAssert.Equal(8f, result.WallHeight);
		TestAssert.Equal(8f, result.WallVertexY);
	}

	private static void MeshVertexCountMatchesGrid()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(result.WorldWidth * result.WorldHeight, result.VertexCount);
		TestAssert.Equal(result.VertexCount, result.TileOverlayCount);
	}

	private static void HeightSampleMatchesMeshVertex()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(result.GroundHeight, result.GroundVertexY);
		TestAssert.True(result.GroundHeight > 0.5f);
	}

	private static void TileOverlayPreservesTileIds()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("ground", result.GroundTileId);
	}
}
