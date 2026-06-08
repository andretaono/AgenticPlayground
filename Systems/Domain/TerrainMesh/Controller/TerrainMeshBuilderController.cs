using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.TerrainMesh.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Domain.TerrainMesh.Controller;

internal sealed class TerrainMeshBuilderController : ITerrainMeshBuilder
{
	private readonly IHeightmapSampler _sampler;

	public TerrainMeshBuilderController(IHeightmapSampler sampler)
	{
		_sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
	}

	public TerrainMeshData Build(Heightmap heightmap, TerrainMeshConfig config)
	{
		if (heightmap is null)
			throw new ArgumentNullException(nameof(heightmap));
		if (config is null)
			throw new ArgumentNullException(nameof(config));

		var vertexCountX = heightmap.Width;
		var vertexCountZ = heightmap.Height;
		var vertices = new List<Vector3>(vertexCountX * vertexCountZ);
		var normals = new List<Vector3>(vertexCountX * vertexCountZ);

		for (var z = 0; z < vertexCountZ; z++)
		{
			for (var x = 0; x < vertexCountX; x++)
			{
				var worldX = x * heightmap.CellSize;
				var worldZ = z * heightmap.CellSize;
				var height = _sampler.SampleBilinear(heightmap, worldX, worldZ) * config.HeightScale;

				vertices.Add(new Vector3(worldX, height, worldZ));
				normals.Add(ComputeNormal(heightmap, config, worldX, worldZ));
			}
		}

		var indices = BuildIndices(vertexCountX, vertexCountZ);
		return new TerrainMeshData(vertices, indices, normals);
	}

	private Vector3 ComputeNormal(Heightmap heightmap, TerrainMeshConfig config, float worldX, float worldZ)
	{
		const float offset = 0.5f;
		var cellOffset = heightmap.CellSize * offset;

		var heightRight = _sampler.SampleBilinear(heightmap, worldX + cellOffset, worldZ) * config.HeightScale;
		var heightLeft = _sampler.SampleBilinear(heightmap, worldX - cellOffset, worldZ) * config.HeightScale;
		var heightForward = _sampler.SampleBilinear(heightmap, worldX, worldZ + cellOffset) * config.HeightScale;
		var heightBack = _sampler.SampleBilinear(heightmap, worldX, worldZ - cellOffset) * config.HeightScale;

		var tangentX = new Vector3(cellOffset * 2f, heightRight - heightLeft, 0f);
		var tangentZ = new Vector3(0f, heightForward - heightBack, cellOffset * 2f);

		return Normalize(Cross(tangentZ, tangentX));
	}

	private static IReadOnlyList<int> BuildIndices(int vertexCountX, int vertexCountZ)
	{
		var quadCountX = vertexCountX - 1;
		var quadCountZ = vertexCountZ - 1;

		if (quadCountX < 1 || quadCountZ < 1)
			return Array.Empty<int>();

		var indices = new List<int>(quadCountX * quadCountZ * 6);

		for (var z = 0; z < quadCountZ; z++)
		{
			for (var x = 0; x < quadCountX; x++)
			{
				var topLeft = z * vertexCountX + x;
				var topRight = topLeft + 1;
				var bottomLeft = topLeft + vertexCountX;
				var bottomRight = bottomLeft + 1;

				indices.Add(topLeft);
				indices.Add(bottomLeft);
				indices.Add(topRight);

				indices.Add(topRight);
				indices.Add(bottomLeft);
				indices.Add(bottomRight);
			}
		}

		return indices;
	}

	private static Vector3 Cross(Vector3 a, Vector3 b) =>
		new(
			a.Y * b.Z - a.Z * b.Y,
			a.Z * b.X - a.X * b.Z,
			a.X * b.Y - a.Y * b.X);

	private static Vector3 Normalize(Vector3 vector)
	{
		var length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		if (length <= 1e-6f)
			return new Vector3(0f, 1f, 0f);

		return new Vector3(vector.X / length, vector.Y / length, vector.Z / length);
	}
}
