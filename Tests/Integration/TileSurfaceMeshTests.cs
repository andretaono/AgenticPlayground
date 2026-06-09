using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class TileSurfaceMeshTests : ITestSuite
{
	public string Name => "tile-surface-mesh";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "builds material groups for demo map", () =>
		{
			var result = new TileSurfaceMeshIntegrationRunner().RunDemoMap();
			TestAssert.True(result.GroupCount > 0);
			TestAssert.True(result.TotalTriangleCount > 0);
			TestAssert.True(result.HasWaterGroup);
			TestAssert.True(result.HasWallGroup);
			TestAssert.True(result.HasGroundGroup);
		});

		registry.Add(Name, "culls shared faces between adjacent ground tiles", () =>
		{
			var result = new TileSurfaceMeshIntegrationRunner().RunAdjacentGroundCulling();
			TestAssert.True(result.HasGroundGroup);
			TestAssert.True(result.SingleGroundTriangleCount > 0);
			TestAssert.True(result.PairGroundTriangleCount > result.SingleGroundTriangleCount);
			TestAssert.True(result.PairGroundTriangleCount < result.SingleGroundTriangleCount * 2);
		});

		registry.Add(Name, "winds triangle fronts toward face normals", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunSingleGroundTriangleWinding());
		});
	}
}
