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

		registry.Add(Name, "preserves hard normals on upward-facing triangles", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunHardTopNormalsPreserved());
		});

		registry.Add(Name, "smooths wall corner normals", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunSoftWallCornerSmoothed());
		});

		registry.Add(Name, "clamps soft normals toward face plane", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunSoftNormalFaceDotClampingApplied());
		});

		registry.Add(Name, "disabled smoothing leaves mesh unchanged", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunSmoothingDisabledMatchesFlat());
		});

		registry.Add(Name, "matches wall and ceiling normals at shared soft seams", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunStructuralWallCeilingSharedNormals());
		});

		registry.Add(Name, "structural smoothing off allows seam normal mismatch", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunStructuralSmoothingOffBreaksSharedNormals());
		});

		registry.Add(Name, "disabled geometry smoothing leaves mesh unchanged", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunGeometrySmoothingDisabledUnchanged());
		});

		registry.Add(Name, "preserves upward Y when geometry smoothing enabled", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunUpwardYPreservedWhenGeometrySmoothing());
		});

		registry.Add(Name, "subdivides soft wall faces when geometry smoothing enabled", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunSoftFacesSubdivide());
		});

		registry.Add(Name, "subdivides upward faces when geometry smoothing enabled", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunHardTopFacesSubdivide());
		});

		registry.Add(Name, "relaxes upward vertices horizontally only", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunUpwardHorizontalRelaxOnly());
		});

		registry.Add(Name, "geometry strength moves shared wall ceiling corners", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunGeometryStrengthMovesSharedCorner());
		});

		registry.Add(Name, "matches ground and cave ground geometry at shared seams", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunGroundCaveGroundSharedGeometry());
		});

		registry.Add(Name, "ground geometry smoothing off allows seam position mismatch", () =>
		{
			TestAssert.True(new TileSurfaceMeshIntegrationRunner().RunGroundGeometrySmoothingOffBreaksSharedPositions());
		});
	}
}
