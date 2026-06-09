using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Tests.Integration.Runners;

public sealed class TileSurfaceMeshIntegrationRunner
{
	public TileSurfaceMeshIntegrationResult RunDemoMap()
	{
		var groundLayer = WorldIntegrationRunner.CreateDemoMap(width: 10, height: 6);
		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(1, 1),
			new WorldPosition(8, 4),
			seedUsed: 42);

		var result = Compose(map);
		return new TileSurfaceMeshIntegrationResult(
			GroupCount: result.Groups.Count,
			TotalTriangleCount: CountTriangles(result),
			HasWaterGroup: HasMaterial(result, SurfaceMaterialId.Water),
			HasWallGroup: HasMaterial(result, SurfaceMaterialId.Wall),
			HasGroundGroup: HasMaterial(result, SurfaceMaterialId.Ground));
	}

	public TileSurfaceMeshIntegrationResult RunAdjacentGroundCulling()
	{
		var groundLayer = new TileId[2, 1]
		{
			{ TileIds.Ground },
			{ TileIds.Ground }
		};

		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(0, 0),
			new WorldPosition(1, 0),
			seedUsed: 7);

		var pairResult = Compose(map);
		var pairTriangles = CountTriangles(pairResult);

		var singleLayer = new TileId[1, 1] { { TileIds.Ground } };
		var singleMap = new GeneratedWorldMap(
			singleLayer,
			new WorldPosition(0, 0),
			new WorldPosition(0, 0),
			seedUsed: 7);
		var singleResult = Compose(singleMap);
		var singleTriangles = CountTriangles(singleResult);

		return new TileSurfaceMeshIntegrationResult(
			GroupCount: pairResult.Groups.Count,
			TotalTriangleCount: pairTriangles,
			HasWaterGroup: false,
			HasWallGroup: false,
			HasGroundGroup: true,
			SingleGroundTriangleCount: singleTriangles,
			PairGroundTriangleCount: pairTriangles);
	}

	public bool RunSingleGroundTriangleWinding()
	{
		var groundLayer = new TileId[1, 1] { { TileIds.Ground } };
		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(0, 0),
			new WorldPosition(0, 0),
			seedUsed: 3);

		var surface = Compose(map);
		var groundMesh = surface.Groups
			.First(group => group.Material == SurfaceMaterialId.Ground)
			.Mesh;

		return AllTrianglesWindOutward(groundMesh);
	}

	private static bool AllTrianglesWindOutward(TerrainMeshData mesh)
	{
		for (var i = 0; i < mesh.Indices.Count; i += 3)
		{
			var i0 = mesh.Indices[i];
			var i1 = mesh.Indices[i + 1];
			var i2 = mesh.Indices[i + 2];

			var v0 = mesh.Vertices[i0];
			var v1 = mesh.Vertices[i1];
			var v2 = mesh.Vertices[i2];
			var edgeA = Subtract(v1, v0);
			var edgeB = Subtract(v2, v0);
			var faceNormal = Cross(edgeA, edgeB);
			var storedNormal = mesh.Normals[i0];

			if (Dot(faceNormal, storedNormal) <= 0f)
				return false;
		}

		return true;
	}

	private static Vector3 Subtract(Vector3 a, Vector3 b) =>
		new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

	private static Vector3 Cross(Vector3 a, Vector3 b) =>
		new(
			a.Y * b.Z - a.Z * b.Y,
			a.Z * b.X - a.X * b.Z,
			a.X * b.Y - a.Y * b.X);

	private static float Dot(Vector3 a, Vector3 b) =>
		a.X * b.X + a.Y * b.Y + a.Z * b.Z;

	private static TileSurfaceMeshResult Compose(GeneratedWorldMap map)
	{
		var composer = new TerrainComposer(new DefaultTileRulesProvider());
		return composer.ComposeFromMap(
			map,
			new WorldTerrainMapping(
				Seed: map.SeedUsed,
				WorldUnitsPerTile: 1f,
				TerrainConfig: new TerrainMeshConfig { HeightScale = 1f },
				ModifierSettings: new TileHeightModifierSettings())).SurfaceMesh!;
	}

	private static int CountTriangles(TileSurfaceMeshResult result)
	{
		var count = 0;
		foreach (var group in result.Groups)
			count += group.Mesh.Indices.Count / 3;

		return count;
	}

	private static bool HasMaterial(TileSurfaceMeshResult result, SurfaceMaterialId material)
	{
		foreach (var group in result.Groups)
		{
			if (group.Material == material)
				return true;
		}

		return false;
	}
}

public sealed record TileSurfaceMeshIntegrationResult(
	int GroupCount,
	int TotalTriangleCount,
	bool HasWaterGroup,
	bool HasWallGroup,
	bool HasGroundGroup,
	int SingleGroundTriangleCount = 0,
	int PairGroundTriangleCount = 0);
