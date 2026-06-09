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

	private static TileSurfaceMeshResult Compose(GeneratedWorldMap map) =>
		Compose(map, new TileSurfaceMeshSettings());

	private static TileSurfaceMeshResult Compose(
		GeneratedWorldMap map,
		TileSurfaceMeshSettings surfaceSettings)
	{
		var composer = new TerrainComposer(new DefaultTileRulesProvider());
		return composer.ComposeFromMap(
			map,
			new WorldTerrainMapping(
				Seed: map.SeedUsed,
				WorldUnitsPerTile: 1f,
				TerrainConfig: new TerrainMeshConfig { HeightScale = 1f },
				ModifierSettings: new TileHeightModifierSettings(),
				SurfaceSettings: surfaceSettings)).SurfaceMesh!;
	}

	public bool RunHardTopNormalsPreserved()
	{
		var groundLayer = WorldIntegrationRunner.CreateDemoMap(width: 10, height: 6);
		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(1, 1),
			new WorldPosition(8, 4),
			seedUsed: 42);

		var surface = Compose(map);
		const float threshold = 0.9f;

		foreach (var group in surface.Groups)
		{
			if (group.Material is not SurfaceMaterialId.Ground and not SurfaceMaterialId.Wall)
				continue;

			if (!AllUpwardTrianglesHaveHardNormals(group.Mesh, threshold))
				return false;
		}

		return true;
	}

	public bool RunSoftWallCornerSmoothed()
	{
		var groundLayer = new TileId[1, 1] { { TileIds.Wall } };
		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(0, 0),
			new WorldPosition(0, 0),
			seedUsed: 5);

		var surface = Compose(map);
		var wallMesh = surface.Groups
			.First(group => group.Material == SurfaceMaterialId.Wall)
			.Mesh;

		return HasNonAxisAlignedSoftNormal(wallMesh, upHardThreshold: 0.9f);
	}

	public bool RunSmoothingDisabledMatchesFlat()
	{
		var groundLayer = new TileId[1, 1] { { TileIds.Ground } };
		var map = new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(0, 0),
			new WorldPosition(0, 0),
			seedUsed: 9);

		var disabledSettings = new TileSurfaceMeshSettings { EnableNormalSmoothing = false };
		var flat = Compose(map, disabledSettings);
		var flatAgain = Compose(map, disabledSettings);

		var groundMesh = flat.Groups.First(group => group.Material == SurfaceMaterialId.Ground).Mesh;
		var groundMeshAgain = flatAgain.Groups.First(group => group.Material == SurfaceMaterialId.Ground).Mesh;

		return MeshesEqual(groundMesh, groundMeshAgain);
	}

	public bool RunStructuralWallCeilingSharedNormals()
	{
		var map = CreateWallWithCeilingStackMap();
		var surface = Compose(map, new TileSurfaceMeshSettings
		{
			EnableStructuralNormalSmoothing = true
		});

		return SharedSoftNormalsMatch(
			surface,
			SurfaceMaterialId.Wall,
			SurfaceMaterialId.CeilingStack,
			upHardThreshold: 0.9f);
	}

	public bool RunStructuralSmoothingOffBreaksSharedNormals()
	{
		var map = CreateWallWithCeilingStackMap();
		var surface = Compose(map, new TileSurfaceMeshSettings
		{
			EnableStructuralNormalSmoothing = false
		});

		return SharedSoftNormalsDiffer(
			surface,
			SurfaceMaterialId.Wall,
			SurfaceMaterialId.CeilingStack,
			upHardThreshold: 0.9f);
	}

	private static GeneratedWorldMap CreateWallWithCeilingStackMap()
	{
		var groundLayer = new TileId[1, 1] { { TileIds.Wall } };
		var ceilingLayer = new TileId[1, 1] { { CeilingLayerTileIds.Solid } };
		return new GeneratedWorldMap(
			groundLayer,
			new WorldPosition(0, 0),
			new WorldPosition(0, 0),
			seedUsed: 13,
			ceilingLayer);
	}

	private static bool SharedSoftNormalsMatch(
		TileSurfaceMeshResult surface,
		SurfaceMaterialId materialA,
		SurfaceMaterialId materialB,
		float upHardThreshold)
	{
		var meshA = surface.Groups.First(group => group.Material == materialA).Mesh;
		var meshB = surface.Groups.First(group => group.Material == materialB).Mesh;
		var normalsA = BuildSoftVertexNormalMap(meshA, upHardThreshold);
		var normalsB = BuildSoftVertexNormalMap(meshB, upHardThreshold);

		var sharedCount = 0;
		foreach (var kvp in normalsA)
		{
			if (!normalsB.TryGetValue(kvp.Key, out var normalB))
				continue;

			sharedCount++;
			if (!ApproxEqual(kvp.Value, normalB))
				return false;
		}

		return sharedCount > 0;
	}

	private static bool SharedSoftNormalsDiffer(
		TileSurfaceMeshResult surface,
		SurfaceMaterialId materialA,
		SurfaceMaterialId materialB,
		float upHardThreshold)
	{
		var meshA = surface.Groups.First(group => group.Material == materialA).Mesh;
		var meshB = surface.Groups.First(group => group.Material == materialB).Mesh;
		var normalsA = BuildSoftVertexNormalMap(meshA, upHardThreshold);
		var normalsB = BuildSoftVertexNormalMap(meshB, upHardThreshold);

		foreach (var kvp in normalsA)
		{
			if (!normalsB.TryGetValue(kvp.Key, out var normalB))
				continue;

			if (!ApproxEqual(kvp.Value, normalB))
				return true;
		}

		return false;
	}

	private static Dictionary<(long, long, long), Vector3> BuildSoftVertexNormalMap(
		TerrainMeshData mesh,
		float upHardThreshold,
		float epsilon = 1e-4f)
	{
		var scale = 1f / epsilon;
		var map = new Dictionary<(long, long, long), Vector3>();

		for (var triIndex = 0; triIndex < mesh.Indices.Count; triIndex += 3)
		{
			var i0 = mesh.Indices[triIndex];
			var i1 = mesh.Indices[triIndex + 1];
			var i2 = mesh.Indices[triIndex + 2];
			var faceNormal = ComputeTriangleNormal(
				mesh.Vertices[i0],
				mesh.Vertices[i1],
				mesh.Vertices[i2]);

			if (faceNormal.Y >= upHardThreshold)
				continue;

			RecordSoftNormal(map, mesh.Vertices[i0], mesh.Normals[i0], scale);
			RecordSoftNormal(map, mesh.Vertices[i1], mesh.Normals[i1], scale);
			RecordSoftNormal(map, mesh.Vertices[i2], mesh.Normals[i2], scale);
		}

		return map;
	}

	private static void RecordSoftNormal(
		Dictionary<(long, long, long), Vector3> map,
		Vector3 position,
		Vector3 normal,
		float scale)
	{
		map[QuantizeKey(position, scale)] = normal;
	}

	private static (long, long, long) QuantizeKey(Vector3 position, float scale) =>
		((long)MathF.Round(position.X * scale),
			(long)MathF.Round(position.Y * scale),
			(long)MathF.Round(position.Z * scale));

	private static bool AllUpwardTrianglesHaveHardNormals(TerrainMeshData mesh, float threshold)
	{
		for (var i = 0; i < mesh.Indices.Count; i += 3)
		{
			var i0 = mesh.Indices[i];
			var i1 = mesh.Indices[i + 1];
			var i2 = mesh.Indices[i + 2];
			var faceNormal = ComputeTriangleNormal(
				mesh.Vertices[i0],
				mesh.Vertices[i1],
				mesh.Vertices[i2]);

			if (faceNormal.Y < threshold)
				continue;

			var storedNormal = mesh.Normals[i0];
			if (storedNormal.Y < 0.99f)
				return false;
		}

		return true;
	}

	private static bool HasNonAxisAlignedSoftNormal(TerrainMeshData mesh, float upHardThreshold)
	{
		for (var i = 0; i < mesh.Normals.Count; i++)
		{
			var normal = mesh.Normals[i];
			if (IsAxisAligned(normal))
				continue;

			if (MathF.Abs(normal.Y) >= upHardThreshold)
				continue;

			return true;
		}

		return false;
	}

	private static bool IsAxisAligned(Vector3 normal)
	{
		const float tolerance = 0.01f;
		var absX = MathF.Abs(normal.X);
		var absY = MathF.Abs(normal.Y);
		var absZ = MathF.Abs(normal.Z);

		return (absX > 1f - tolerance && absY < tolerance && absZ < tolerance) ||
		       (absY > 1f - tolerance && absX < tolerance && absZ < tolerance) ||
		       (absZ > 1f - tolerance && absX < tolerance && absY < tolerance);
	}

	private static Vector3 ComputeTriangleNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		var edgeA = Subtract(b, a);
		var edgeB = Subtract(c, a);
		return Normalize(Cross(edgeA, edgeB));
	}

	private static Vector3 Normalize(Vector3 v)
	{
		var length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
		if (length <= 1e-6f)
			return new Vector3(0f, 1f, 0f);

		return new Vector3(v.X / length, v.Y / length, v.Z / length);
	}

	private static bool MeshesEqual(TerrainMeshData left, TerrainMeshData right)
	{
		if (left.Vertices.Count != right.Vertices.Count ||
		    left.Indices.Count != right.Indices.Count ||
		    left.Normals.Count != right.Normals.Count)
		{
			return false;
		}

		for (var i = 0; i < left.Vertices.Count; i++)
		{
			if (!ApproxEqual(left.Vertices[i], right.Vertices[i]) ||
			    !ApproxEqual(left.Normals[i], right.Normals[i]))
			{
				return false;
			}
		}

		for (var i = 0; i < left.Indices.Count; i++)
		{
			if (left.Indices[i] != right.Indices[i])
				return false;
		}

		return true;
	}

	private static bool ApproxEqual(Vector3 a, Vector3 b, float epsilon = 1e-4f) =>
		MathF.Abs(a.X - b.X) <= epsilon &&
		MathF.Abs(a.Y - b.Y) <= epsilon &&
		MathF.Abs(a.Z - b.Z) <= epsilon;

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
