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
		registry.Add(Name, "ground tiles use ground height", GroundTilesUseGroundHeight);
		registry.Add(Name, "heightmap tile ids match world tiles", HeightmapTileIdsMatchWorldTiles);
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
	}

	private static void WallTilesUseWallHeight()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("wall", result.WallTileId);
		TestAssert.Equal(1f, result.WallHeight);
	}

	private static void GroundTilesUseGroundHeight()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal(0f, result.GroundHeight);
	}

	private static void HeightmapTileIdsMatchWorldTiles()
	{
		var result = new WorldTerrainMeshIntegrationRunner().Run();

		TestAssert.Equal("ground", result.GroundTileId);
	}
}
