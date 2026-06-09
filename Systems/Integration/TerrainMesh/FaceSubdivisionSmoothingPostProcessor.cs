using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class FaceSubdivisionSmoothingPostProcessor : ITileSurfaceMeshPostProcessor
{
	private const float PointEpsilon = 1e-5f;

	public TileSurfaceMeshResult Process(
		TileSurfaceMeshResult mesh,
		TileSurfaceMeshSettings settings,
		float cellSize)
	{
		if (mesh is null)
			throw new ArgumentNullException(nameof(mesh));
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));

		if (!ShouldRun(settings))
			return mesh;

		_ = cellSize;
		var groundGroups = new List<TileSurfaceMeshGroup>();
		var structuralGroups = new List<TileSurfaceMeshGroup>();
		var independentGroups = new List<TileSurfaceMeshGroup>();
		foreach (var group in mesh.Groups)
		{
			if (IsGroundMaterial(group.Material))
				groundGroups.Add(group);
			else if (IsStructuralMaterial(group.Material))
				structuralGroups.Add(group);
			else
				independentGroups.Add(group);
		}

		var resultGroups = new List<TileSurfaceMeshGroup>(mesh.Groups.Count);
		AppendProcessedGroups(independentGroups, resultGroups, settings, union: false);
		AppendProcessedGroups(
			groundGroups,
			resultGroups,
			settings,
			union: settings.EnableGroundGeometrySmoothing);
		AppendProcessedGroups(
			structuralGroups,
			resultGroups,
			settings,
			union: settings.EnableStructuralGeometrySmoothing);

		return new TileSurfaceMeshResult(resultGroups);
	}

	private static bool ShouldRun(TileSurfaceMeshSettings settings) =>
		settings.EnableGeometrySmoothing &&
		settings.GeometrySmoothDivisions > 0 &&
		settings.GeometrySmoothStrength > 0f;

	private static bool IsStructuralMaterial(SurfaceMaterialId material) =>
		material is SurfaceMaterialId.Wall
			or SurfaceMaterialId.CeilingStack
			or SurfaceMaterialId.CeilingCap;

	private static bool IsGroundMaterial(SurfaceMaterialId material) =>
		material is SurfaceMaterialId.Ground or SurfaceMaterialId.CaveGround;

	private static void AppendProcessedGroups(
		List<TileSurfaceMeshGroup> groups,
		List<TileSurfaceMeshGroup> resultGroups,
		TileSurfaceMeshSettings settings,
		bool union)
	{
		if (groups.Count == 0)
			return;

		if (!union || groups.Count == 1)
		{
			foreach (var group in groups)
			{
				resultGroups.Add(group with
				{
					Mesh = ProcessSingleMesh(group.Mesh, settings)
				});
			}

			return;
		}

		var subdivided = groups
			.Select(group => (group, SubdivideMesh(group.Mesh, settings)))
			.ToList();
		RelaxWeldedUnion(subdivided, settings);
		foreach (var (group, processedMesh) in subdivided)
			resultGroups.Add(group with { Mesh = processedMesh });
	}

	private static TerrainMeshData ProcessSingleMesh(
		TerrainMeshData source,
		TileSurfaceMeshSettings settings)
	{
		var subdivided = SubdivideMesh(source, settings);
		return RelaxMesh(subdivided, settings);
	}

	private static TerrainMeshData SubdivideMesh(
		TerrainMeshData source,
		TileSurfaceMeshSettings settings)
	{
		var quads = RecoverQuads(source);
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var indices = new List<int>();
		var divisions = settings.GeometrySmoothDivisions;

		foreach (var quad in quads)
			EmitSubdividedQuad(quad, divisions, vertices, normals, indices);

		return TerrainMeshData.Create(vertices, indices, normals);
	}

	private static void RelaxWeldedUnion(
		List<(TileSurfaceMeshGroup Group, TerrainMeshData Mesh)> subdivided,
		TileSurfaceMeshSettings settings)
	{
		var scale = 1f / settings.WeldEpsilon;
		var weldCount = 0;
		var weldPositions = new List<Vector3>();
		var weldIndexByKey = new Dictionary<(long, long, long), int>();
		var meshVertexToWeld = new List<int[]>();

		for (var meshIndex = 0; meshIndex < subdivided.Count; meshIndex++)
		{
			var mesh = subdivided[meshIndex].Mesh;
			var remap = new int[mesh.Vertices.Count];
			for (var i = 0; i < mesh.Vertices.Count; i++)
			{
				var key = QuantizeKey(mesh.Vertices[i], scale);
				if (!weldIndexByKey.TryGetValue(key, out var weldIndex))
				{
					weldIndex = weldCount++;
					weldIndexByKey[key] = weldIndex;
					weldPositions.Add(mesh.Vertices[i]);
				}

				remap[i] = weldIndex;
			}

			meshVertexToWeld.Add(remap);
		}

		var adjacency = BuildWeldedAdjacency(
			subdivided.Select(entry => entry.Mesh),
			meshVertexToWeld,
			weldCount);

		RelaxWeldedPositions(
			weldPositions,
			adjacency,
			settings.GeometrySmoothStrength,
			BuildHorizontalWelds(
				subdivided.Select(entry => entry.Mesh),
				meshVertexToWeld,
				settings.UpHardNormalThreshold));

		for (var meshIndex = 0; meshIndex < subdivided.Count; meshIndex++)
		{
			var mesh = subdivided[meshIndex].Mesh;
			var remap = meshVertexToWeld[meshIndex];
			var newVertices = new List<Vector3>(mesh.Vertices.Count);
			for (var i = 0; i < mesh.Vertices.Count; i++)
				newVertices.Add(weldPositions[remap[i]]);

			subdivided[meshIndex] = (
				subdivided[meshIndex].Group,
				RebuildMeshWithVertices(mesh, newVertices));
		}
	}

	private static TerrainMeshData RelaxMesh(
		TerrainMeshData mesh,
		TileSurfaceMeshSettings settings)
	{
		var scale = 1f / settings.WeldEpsilon;
		var weldPositions = new List<Vector3>();
		var remap = new int[mesh.Vertices.Count];
		var weldIndexByKey = new Dictionary<(long, long, long), int>();

		for (var i = 0; i < mesh.Vertices.Count; i++)
		{
			var key = QuantizeKey(mesh.Vertices[i], scale);
			if (!weldIndexByKey.TryGetValue(key, out var weldIndex))
			{
				weldIndex = weldPositions.Count;
				weldIndexByKey[key] = weldIndex;
				weldPositions.Add(mesh.Vertices[i]);
			}

			remap[i] = weldIndex;
		}

		var adjacency = BuildWeldedAdjacency(new[] { mesh }, new[] { remap }, weldPositions.Count);
		RelaxWeldedPositions(
			weldPositions,
			adjacency,
			settings.GeometrySmoothStrength,
			BuildHorizontalWelds(new[] { mesh }, new[] { remap }, settings.UpHardNormalThreshold));

		var newVertices = new List<Vector3>(mesh.Vertices.Count);
		for (var i = 0; i < mesh.Vertices.Count; i++)
			newVertices.Add(weldPositions[remap[i]]);

		return RebuildMeshWithVertices(mesh, newVertices);
	}

	private static List<HashSet<int>> BuildWeldedAdjacency(
		IEnumerable<TerrainMeshData> meshes,
		IReadOnlyList<int[]> meshVertexToWeld,
		int weldCount)
	{
		var adjacency = new List<HashSet<int>>(weldCount);
		for (var i = 0; i < weldCount; i++)
			adjacency.Add(new HashSet<int>());

		var meshIndex = 0;
		foreach (var mesh in meshes)
		{
			var remap = meshVertexToWeld[meshIndex++];
			for (var triIndex = 0; triIndex < mesh.Indices.Count; triIndex += 3)
			{
				var w0 = remap[mesh.Indices[triIndex]];
				var w1 = remap[mesh.Indices[triIndex + 1]];
				var w2 = remap[mesh.Indices[triIndex + 2]];
				Connect(adjacency, w0, w1);
				Connect(adjacency, w1, w2);
				Connect(adjacency, w2, w0);
			}
		}

		return adjacency;
	}

	private static void Connect(List<HashSet<int>> adjacency, int a, int b)
	{
		if (a == b)
			return;

		adjacency[a].Add(b);
		adjacency[b].Add(a);
	}

	private static void RelaxWeldedPositions(
		List<Vector3> positions,
		List<HashSet<int>> adjacency,
		float strength,
		HashSet<int> horizontalWelds)
	{
		var originalY = new float[positions.Count];
		for (var i = 0; i < positions.Count; i++)
			originalY[i] = positions[i].Y;

		var deltas = new Vector3[positions.Count];
		for (var i = 0; i < positions.Count; i++)
		{
			var neighbors = adjacency[i];
			if (neighbors.Count == 0)
				continue;

			var average = new Vector3(0f, 0f, 0f);
			foreach (var neighbor in neighbors)
				average = Add(average, positions[neighbor]);

			average = Scale(average, 1f / neighbors.Count);
			deltas[i] = Subtract(average, positions[i]);
		}

		for (var i = 0; i < positions.Count; i++)
		{
			var delta = Scale(deltas[i], strength);
			if (horizontalWelds.Contains(i))
			{
				positions[i] = new Vector3(
					positions[i].X + delta.X,
					originalY[i],
					positions[i].Z + delta.Z);
				continue;
			}

			positions[i] = Add(positions[i], delta);
		}
	}

	private static HashSet<int> BuildHorizontalWelds(
		IEnumerable<TerrainMeshData> meshes,
		IReadOnlyList<int[]> meshVertexToWeld,
		float upHardThreshold)
	{
		var horizontalWelds = new HashSet<int>();
		var meshIndex = 0;
		foreach (var mesh in meshes)
		{
			var remap = meshVertexToWeld[meshIndex++];
			for (var triIndex = 0; triIndex < mesh.Indices.Count; triIndex += 3)
			{
				var i0 = mesh.Indices[triIndex];
				var i1 = mesh.Indices[triIndex + 1];
				var i2 = mesh.Indices[triIndex + 2];
				var faceNormal = ComputeTriangleNormal(
					mesh.Vertices[i0],
					mesh.Vertices[i1],
					mesh.Vertices[i2]);

				if (faceNormal.Y < upHardThreshold)
					continue;

				horizontalWelds.Add(remap[i0]);
				horizontalWelds.Add(remap[i1]);
				horizontalWelds.Add(remap[i2]);
			}
		}

		return horizontalWelds;
	}

	private static TerrainMeshData RebuildMeshWithVertices(
		TerrainMeshData mesh,
		IReadOnlyList<Vector3> vertices)
	{
		var indices = mesh.Indices.ToList();
		var normals = new List<Vector3>(vertices.Count);
		for (var i = 0; i < vertices.Count; i++)
			normals.Add(new Vector3(0f, 1f, 0f));

		RecomputeFlatNormals(vertices, indices, normals);
		return TerrainMeshData.Create(vertices, indices, normals);
	}

	private static void RecomputeFlatNormals(
		IReadOnlyList<Vector3> vertices,
		IReadOnlyList<int> indices,
		IList<Vector3> normals)
	{
		for (var triIndex = 0; triIndex < indices.Count; triIndex += 3)
		{
			var i0 = indices[triIndex];
			var i1 = indices[triIndex + 1];
			var i2 = indices[triIndex + 2];
			var normal = ComputeTriangleNormal(vertices[i0], vertices[i1], vertices[i2]);
			normals[i0] = normal;
			normals[i1] = normal;
			normals[i2] = normal;
		}
	}

	private static void EmitSubdividedQuad(
		RecoveredQuad quad,
		int divisions,
		List<Vector3> vertices,
		List<Vector3> normals,
		List<int> indices)
	{
		var cellsPerAxis = divisions + 1;
		var gridSize = cellsPerAxis + 1;
		var gridStart = vertices.Count;
		for (var row = 0; row < gridSize; row++)
		{
			var v = row / (float)cellsPerAxis;
			for (var col = 0; col < gridSize; col++)
			{
				var u = col / (float)cellsPerAxis;
				vertices.Add(SampleQuad(quad, u, v));
				normals.Add(quad.Normal);
			}
		}

		for (var row = 0; row < cellsPerAxis; row++)
		for (var col = 0; col < cellsPerAxis; col++)
		{
			var i0 = gridStart + row * gridSize + col;
			var i1 = i0 + 1;
			var i2 = i0 + gridSize + 1;
			var i3 = i0 + gridSize;
			indices.Add(i0);
			indices.Add(i1);
			indices.Add(i2);
			indices.Add(i0);
			indices.Add(i2);
			indices.Add(i3);
		}
	}

	private static Vector3 SampleQuad(RecoveredQuad quad, float u, float v)
	{
		var a = Lerp(quad.C0, quad.C1, u);
		var b = Lerp(quad.C3, quad.C2, u);
		return Lerp(a, b, v);
	}

	private static List<RecoveredQuad> RecoverQuads(TerrainMeshData mesh)
	{
		var triangleCount = mesh.Indices.Count / 3;
		var used = new bool[triangleCount];
		var quads = new List<RecoveredQuad>();

		for (var triIndex = 0; triIndex < triangleCount; triIndex++)
		{
			if (used[triIndex])
				continue;

			var p0 = mesh.Vertices[mesh.Indices[triIndex * 3]];
			var p1 = mesh.Vertices[mesh.Indices[triIndex * 3 + 1]];
			var p2 = mesh.Vertices[mesh.Indices[triIndex * 3 + 2]];
			var normal = ComputeTriangleNormal(p0, p1, p2);

			var partnerIndex = FindPartnerTriangle(mesh, triIndex, p0, p1, p2, normal, used);
			if (partnerIndex >= 0)
			{
				used[triIndex] = true;
				used[partnerIndex] = true;
				var q0 = mesh.Vertices[mesh.Indices[partnerIndex * 3]];
				var q1 = mesh.Vertices[mesh.Indices[partnerIndex * 3 + 1]];
				var q2 = mesh.Vertices[mesh.Indices[partnerIndex * 3 + 2]];
				quads.Add(BuildQuadFromTrianglePair(p0, p1, p2, q0, q1, q2, normal));
				continue;
			}

			used[triIndex] = true;
			quads.Add(new RecoveredQuad(p0, p1, p2, p2, normal));
		}

		return quads;
	}

	private static int FindPartnerTriangle(
		TerrainMeshData mesh,
		int triIndex,
		Vector3 p0,
		Vector3 p1,
		Vector3 p2,
		Vector3 normal,
		bool[] used)
	{
		var triangleCount = mesh.Indices.Count / 3;
		for (var candidate = triIndex + 1; candidate < triangleCount; candidate++)
		{
			if (used[candidate])
				continue;

			var q0 = mesh.Vertices[mesh.Indices[candidate * 3]];
			var q1 = mesh.Vertices[mesh.Indices[candidate * 3 + 1]];
			var q2 = mesh.Vertices[mesh.Indices[candidate * 3 + 2]];
			var partnerNormal = ComputeTriangleNormal(q0, q1, q2);
			if (Dot(partnerNormal, normal) < 0.999f)
				continue;

			if (CountSharedPoints(p0, p1, p2, q0, q1, q2) == 2)
				return candidate;
		}

		return -1;
	}

	private static int CountSharedPoints(
		Vector3 p0,
		Vector3 p1,
		Vector3 p2,
		Vector3 q0,
		Vector3 q1,
		Vector3 q2)
	{
		var tri2 = new[] { q0, q1, q2 };
		var count = 0;
		if (ContainsPoint(tri2, p0)) count++;
		if (ContainsPoint(tri2, p1)) count++;
		if (ContainsPoint(tri2, p2)) count++;
		return count;
	}

	private static RecoveredQuad BuildQuadFromTrianglePair(
		Vector3 p0,
		Vector3 p1,
		Vector3 p2,
		Vector3 q0,
		Vector3 q1,
		Vector3 q2,
		Vector3 normal)
	{
		var tri1 = new[] { p0, p1, p2 };
		var tri2 = new[] { q0, q1, q2 };
		Vector3? unique1 = null;
		Vector3? unique2 = null;
		var shared = new List<Vector3>(2);

		foreach (var point in tri1)
		{
			if (ContainsPoint(tri2, point))
				shared.Add(point);
			else
				unique1 = point;
		}

		foreach (var point in tri2)
		{
			if (!ContainsPoint(tri1, point))
				unique2 = point;
		}

		if (shared.Count != 2 || unique1 is null || unique2 is null)
			return new RecoveredQuad(p0, p1, p2, p2, normal);

		return OrderQuad(shared[0], shared[1], unique1.Value, unique2.Value, normal);
	}

	private static RecoveredQuad OrderQuad(
		Vector3 diagonalA,
		Vector3 diagonalB,
		Vector3 unique1,
		Vector3 unique2,
		Vector3 normal)
	{
		var candidates = new (Vector3 C0, Vector3 C1, Vector3 C2, Vector3 C3)[]
		{
			(diagonalA, unique1, diagonalB, unique2),
			(diagonalA, unique2, diagonalB, unique1),
			(diagonalB, unique1, diagonalA, unique2),
			(diagonalB, unique2, diagonalA, unique1)
		};

		foreach (var (c0, c1, c2, c3) in candidates)
		{
			var faceNormal = ComputeTriangleNormal(c0, c1, c2);
			if (Dot(faceNormal, normal) >= 0.999f)
				return new RecoveredQuad(c0, c1, c2, c3, normal);
		}

		var fallback = candidates[0];
		return new RecoveredQuad(fallback.C0, fallback.C1, fallback.C2, fallback.C3, normal);
	}

	private static bool ContainsPoint(IReadOnlyList<Vector3> points, Vector3 target)
	{
		foreach (var point in points)
		{
			if (SharesPoint(point, target))
				return true;
		}

		return false;
	}

	private static bool SharesPoint(Vector3 a, Vector3 b) =>
		MathF.Abs(a.X - b.X) <= PointEpsilon &&
		MathF.Abs(a.Y - b.Y) <= PointEpsilon &&
		MathF.Abs(a.Z - b.Z) <= PointEpsilon;

	private static (long, long, long) QuantizeKey(Vector3 position, float scale) =>
		((long)MathF.Round(position.X * scale),
			(long)MathF.Round(position.Y * scale),
			(long)MathF.Round(position.Z * scale));

	private static Vector3 ComputeTriangleNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		var ab = Subtract(b, a);
		var ac = Subtract(c, a);
		return Normalize(Cross(ab, ac));
	}

	private static Vector3 Lerp(Vector3 a, Vector3 b, float t) =>
		Add(a, Scale(Subtract(b, a), t));

	private static Vector3 Add(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

	private static Vector3 Subtract(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

	private static Vector3 Scale(Vector3 v, float scale) => new(v.X * scale, v.Y * scale, v.Z * scale);

	private static Vector3 Cross(Vector3 a, Vector3 b) =>
		new(
			a.Y * b.Z - a.Z * b.Y,
			a.Z * b.X - a.X * b.Z,
			a.X * b.Y - a.Y * b.X);

	private static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

	private static Vector3 Normalize(Vector3 v)
	{
		var length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
		if (length <= 1e-6f)
			return new Vector3(0f, 1f, 0f);

		return Scale(v, 1f / length);
	}

	private readonly record struct RecoveredQuad(
		Vector3 C0,
		Vector3 C1,
		Vector3 C2,
		Vector3 C3,
		Vector3 Normal);
}
