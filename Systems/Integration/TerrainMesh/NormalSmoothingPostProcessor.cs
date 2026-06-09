using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.TerrainMesh;

public sealed class NormalSmoothingPostProcessor : ITileSurfaceMeshPostProcessor
{
	public TileSurfaceMeshResult Process(
		TileSurfaceMeshResult mesh,
		TileSurfaceMeshSettings settings,
		float cellSize)
	{
		if (mesh is null)
			throw new ArgumentNullException(nameof(mesh));
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));

		if (!settings.EnableNormalSmoothing)
			return mesh;

		_ = cellSize;
		var structuralGroups = new List<TileSurfaceMeshGroup>();
		var otherGroups = new List<TileSurfaceMeshGroup>();
		foreach (var group in mesh.Groups)
		{
			if (IsStructuralMaterial(group.Material))
				structuralGroups.Add(group);
			else
				otherGroups.Add(group);
		}

		var resultGroups = new List<TileSurfaceMeshGroup>(mesh.Groups.Count);
		foreach (var group in otherGroups)
			resultGroups.Add(group with { Mesh = SmoothMesh(group.Mesh, settings, expandSoftCorners: true) });

		if (structuralGroups.Count == 0)
			return new TileSurfaceMeshResult(resultGroups);

		if (settings.EnableStructuralNormalSmoothing)
		{
			var merged = MergeMeshes(structuralGroups.Select(group => group.Mesh));
			var smoothMerged = SmoothMesh(merged, settings, expandSoftCorners: false);
			var softNormalMap = BuildSoftNormalPositionMap(smoothMerged, settings);
			foreach (var group in structuralGroups)
			{
				resultGroups.Add(group with
				{
					Mesh = RebuildWithSharedSoftNormals(group.Mesh, settings, softNormalMap)
				});
			}
		}
		else
		{
			foreach (var group in structuralGroups)
				resultGroups.Add(group with { Mesh = SmoothMesh(group.Mesh, settings, expandSoftCorners: true) });
		}

		return new TileSurfaceMeshResult(resultGroups);
	}

	private static bool IsStructuralMaterial(SurfaceMaterialId material) =>
		material is SurfaceMaterialId.Wall
			or SurfaceMaterialId.CeilingStack
			or SurfaceMaterialId.CeilingCap;

	private static TerrainMeshData MergeMeshes(IEnumerable<TerrainMeshData> meshes)
	{
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		foreach (var mesh in meshes)
		{
			var offset = vertices.Count;
			vertices.AddRange(mesh.Vertices);
			normals.AddRange(mesh.Normals);
			foreach (var index in mesh.Indices)
				indices.Add(offset + index);
		}

		return TerrainMeshData.Create(vertices, indices, normals);
	}

	private static Dictionary<(long, long, long), Vector3> BuildSoftNormalPositionMap(
		TerrainMeshData smoothed,
		TileSurfaceMeshSettings settings)
	{
		var threshold = settings.UpHardNormalThreshold;
		var scale = 1f / settings.WeldEpsilon;
		var map = new Dictionary<(long, long, long), Vector3>();
		var softVertexIndices = CollectSoftVertexIndices(smoothed, threshold);

		for (var i = 0; i < smoothed.Normals.Count; i++)
		{
			if (!softVertexIndices.Contains(i))
				continue;

			var key = QuantizeKey(smoothed.Vertices[i], scale);
			map[key] = smoothed.Normals[i];
		}

		return map;
	}

	private static HashSet<int> CollectSoftVertexIndices(TerrainMeshData mesh, float threshold)
	{
		var softVertices = new HashSet<int>();
		for (var triIndex = 0; triIndex < mesh.Indices.Count; triIndex += 3)
		{
			var i0 = mesh.Indices[triIndex];
			var i1 = mesh.Indices[triIndex + 1];
			var i2 = mesh.Indices[triIndex + 2];
			var faceNormal = ComputeTriangleNormal(
				mesh.Vertices[i0],
				mesh.Vertices[i1],
				mesh.Vertices[i2]);

			if (faceNormal.Y >= threshold)
				continue;

			softVertices.Add(i0);
			softVertices.Add(i1);
			softVertices.Add(i2);
		}

		return softVertices;
	}

	private static TerrainMeshData RebuildWithSharedSoftNormals(
		TerrainMeshData source,
		TileSurfaceMeshSettings settings,
		IReadOnlyDictionary<(long, long, long), Vector3> softNormalMap)
	{
		var threshold = settings.UpHardNormalThreshold;
		var scale = 1f / settings.WeldEpsilon;
		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		for (var triIndex = 0; triIndex < source.Indices.Count; triIndex += 3)
		{
			var i0 = source.Indices[triIndex];
			var i1 = source.Indices[triIndex + 1];
			var i2 = source.Indices[triIndex + 2];
			var p0 = source.Vertices[i0];
			var p1 = source.Vertices[i1];
			var p2 = source.Vertices[i2];
			var faceNormal = ComputeTriangleNormal(p0, p1, p2);

			if (faceNormal.Y >= threshold)
			{
				var start = vertices.Count;
				vertices.Add(p0);
				vertices.Add(p1);
				vertices.Add(p2);
				normals.Add(faceNormal);
				normals.Add(faceNormal);
				normals.Add(faceNormal);
				indices.Add(start);
				indices.Add(start + 1);
				indices.Add(start + 2);
				continue;
			}

			var n0 = ResolveSoftNormal(p0, faceNormal, softNormalMap, scale, settings);
			var n1 = ResolveSoftNormal(p1, faceNormal, softNormalMap, scale, settings);
			var n2 = ResolveSoftNormal(p2, faceNormal, softNormalMap, scale, settings);
			var startSoft = vertices.Count;
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			normals.Add(n0);
			normals.Add(n1);
			normals.Add(n2);
			indices.Add(startSoft);
			indices.Add(startSoft + 1);
			indices.Add(startSoft + 2);
		}

		return TerrainMeshData.Create(vertices, indices, normals);
	}

	private static Vector3 ResolveSoftNormal(
		Vector3 position,
		Vector3 faceNormal,
		IReadOnlyDictionary<(long, long, long), Vector3> softNormalMap,
		float scale,
		TileSurfaceMeshSettings settings)
	{
		var sharedNormal = softNormalMap.TryGetValue(QuantizeKey(position, scale), out var smoothed)
			? smoothed
			: faceNormal;

		return ClampSoftNormalTowardFace(sharedNormal, faceNormal, settings.SoftNormalMinFaceDot);
	}

	private static TerrainMeshData SmoothMesh(
		TerrainMeshData source,
		TileSurfaceMeshSettings settings,
		bool expandSoftCorners)
	{
		var threshold = settings.UpHardNormalThreshold;
		var epsilon = settings.WeldEpsilon;
		var scale = 1f / epsilon;

		var hardVertices = new List<Vector3>();
		var hardNormals = new List<Vector3>();
		var hardIndices = new List<int>();

		var softVertices = new List<Vector3>();
		var softNormalSums = new List<Vector3>();
		var softWeldMap = new Dictionary<(long, long, long), int>();
		var softTriangles = new List<(int I0, int I1, int I2, Vector3 FaceNormal)>();

		for (var triIndex = 0; triIndex < source.Indices.Count; triIndex += 3)
		{
			var i0 = source.Indices[triIndex];
			var i1 = source.Indices[triIndex + 1];
			var i2 = source.Indices[triIndex + 2];
			var p0 = source.Vertices[i0];
			var p1 = source.Vertices[i1];
			var p2 = source.Vertices[i2];
			var faceNormal = ComputeTriangleNormal(p0, p1, p2);

			if (faceNormal.Y >= threshold)
			{
				var start = hardVertices.Count;
				hardVertices.Add(p0);
				hardVertices.Add(p1);
				hardVertices.Add(p2);
				hardNormals.Add(faceNormal);
				hardNormals.Add(faceNormal);
				hardNormals.Add(faceNormal);
				hardIndices.Add(start);
				hardIndices.Add(start + 1);
				hardIndices.Add(start + 2);
				continue;
			}

			var weldedI0 = AccumulateSoftVertex(p0, faceNormal, softVertices, softNormalSums, softWeldMap, scale);
			var weldedI1 = AccumulateSoftVertex(p1, faceNormal, softVertices, softNormalSums, softWeldMap, scale);
			var weldedI2 = AccumulateSoftVertex(p2, faceNormal, softVertices, softNormalSums, softWeldMap, scale);
			softTriangles.Add((weldedI0, weldedI1, weldedI2, faceNormal));
		}

		var vertices = new List<Vector3>(hardVertices.Count + softVertices.Count);
		var normals = new List<Vector3>(hardNormals.Count + softVertices.Count);
		var indices = new List<int>(hardIndices.Count + softTriangles.Count * 3);

		vertices.AddRange(hardVertices);
		normals.AddRange(hardNormals);
		indices.AddRange(hardIndices);

		if (expandSoftCorners)
		{
			foreach (var (i0, i1, i2, faceNormal) in softTriangles)
			{
				var start = vertices.Count;
				vertices.Add(softVertices[i0]);
				vertices.Add(softVertices[i1]);
				vertices.Add(softVertices[i2]);
				normals.Add(ClampSoftNormalTowardFace(
					Normalize(softNormalSums[i0]),
					faceNormal,
					settings.SoftNormalMinFaceDot));
				normals.Add(ClampSoftNormalTowardFace(
					Normalize(softNormalSums[i1]),
					faceNormal,
					settings.SoftNormalMinFaceDot));
				normals.Add(ClampSoftNormalTowardFace(
					Normalize(softNormalSums[i2]),
					faceNormal,
					settings.SoftNormalMinFaceDot));
				indices.Add(start);
				indices.Add(start + 1);
				indices.Add(start + 2);
			}
		}
		else
		{
			var softIndexOffset = hardVertices.Count;
			for (var i = 0; i < softNormalSums.Count; i++)
			{
				vertices.Add(softVertices[i]);
				normals.Add(Normalize(softNormalSums[i]));
			}

			foreach (var (i0, i1, i2, _) in softTriangles)
			{
				indices.Add(softIndexOffset + i0);
				indices.Add(softIndexOffset + i1);
				indices.Add(softIndexOffset + i2);
			}
		}

		return TerrainMeshData.Create(vertices, indices, normals);
	}

	private static int AccumulateSoftVertex(
		Vector3 position,
		Vector3 faceNormal,
		List<Vector3> vertices,
		List<Vector3> normalSums,
		Dictionary<(long, long, long), int> weldMap,
		float scale)
	{
		var key = QuantizeKey(position, scale);
		if (weldMap.TryGetValue(key, out var index))
		{
			normalSums[index] = Add(normalSums[index], faceNormal);
			return index;
		}

		index = vertices.Count;
		weldMap[key] = index;
		vertices.Add(position);
		normalSums.Add(faceNormal);
		return index;
	}

	private static Vector3 ClampSoftNormalTowardFace(
		Vector3 smoothedNormal,
		Vector3 faceNormal,
		float minDot)
	{
		smoothedNormal = Normalize(smoothedNormal);
		if (minDot <= 0f)
			return smoothedNormal;

		var dot = Dot(smoothedNormal, faceNormal);
		if (dot + 1e-5f >= minDot)
			return smoothedNormal;

		var t = (minDot - dot) / (1f - dot + 1e-6f);
		t = Math.Clamp(t, 0f, 1f);
		return Normalize(Lerp(smoothedNormal, faceNormal, t));
	}

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

	private static Vector3 Add(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

	private static Vector3 Subtract(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

	private static Vector3 Cross(Vector3 a, Vector3 b) =>
		new(
			a.Y * b.Z - a.Z * b.Y,
			a.Z * b.X - a.X * b.Z,
			a.X * b.Y - a.Y * b.X);

	private static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

	private static Vector3 Lerp(Vector3 a, Vector3 b, float t) =>
		new(
			a.X + (b.X - a.X) * t,
			a.Y + (b.Y - a.Y) * t,
			a.Z + (b.Z - a.Z) * t);

	private static Vector3 Normalize(Vector3 v)
	{
		var length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
		if (length <= 1e-6f)
			return new Vector3(0f, 1f, 0f);

		return new Vector3(v.X / length, v.Y / length, v.Z / length);
	}
}
