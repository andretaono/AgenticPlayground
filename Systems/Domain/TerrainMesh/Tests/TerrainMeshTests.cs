using System.Security.Cryptography;
using System.Text;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.TerrainMesh.Tests;

public sealed class TerrainMeshTests : ITestSuite
{
	public string Name => "unit/terrain-mesh";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "same seed produces identical heightmap", SameSeedProducesIdenticalHeightmap);
		registry.Add(Name, "generated samples stay within configured range", GeneratedSamplesStayInRange);
		registry.Add(Name, "known heightmap produces expected vertex heights", KnownHeightmapProducesExpectedVertices);
		registry.Add(Name, "mesh topology matches grid dimensions", MeshTopologyMatchesGrid);
		registry.Add(Name, "flat heightmap normals point up", FlatHeightmapNormalsPointUp);
		registry.Add(Name, "mesh vertices are deterministic golden hash", MeshVerticesGoldenHash);
	}

	private static TerrainMeshSystem CreateSystem() => new();

	private static TerrainMeshConfig DefaultConfig() => new()
	{
		CellSize = 1f,
		HeightScale = 1f,
		MinHeight = 0f,
		MaxHeight = 10f,
		NoiseFrequency = 0.1f,
		NoiseOctaves = 3
	};

	private static void SameSeedProducesIdenticalHeightmap()
	{
		var system = CreateSystem();
		var config = DefaultConfig();

		var first = system.Generator.Generate(seed: 42, width: 8, height: 8, config);
		var second = system.Generator.Generate(seed: 42, width: 8, height: 8, config);

		for (var y = 0; y < first.Height; y++)
		for (var x = 0; x < first.Width; x++)
			TestAssert.Equal(first.Sample(x, y), second.Sample(x, y));
	}

	private static void GeneratedSamplesStayInRange()
	{
		var system = CreateSystem();
		var config = new TerrainMeshConfig { MinHeight = 2f, MaxHeight = 7f };

		var heightmap = system.Generator.Generate(seed: 7, width: 16, height: 12, config);

		for (var y = 0; y < heightmap.Height; y++)
		{
			for (var x = 0; x < heightmap.Width; x++)
			{
				var sample = heightmap.Sample(x, y);
				TestAssert.False(float.IsNaN(sample));
				TestAssert.False(float.IsInfinity(sample));
				TestAssert.True(sample >= config.MinHeight);
				TestAssert.True(sample <= config.MaxHeight);
			}
		}
	}

	private static void KnownHeightmapProducesExpectedVertices()
	{
		var system = CreateSystem();
		var heightmap = Heightmap.FromSamples(new[,]
		{
			{ 1f, 2f },
			{ 3f, 4f }
		});

		var mesh = system.MeshBuilder.Build(heightmap, new TerrainMeshConfig { HeightScale = 1f });

		TestAssert.Equal(4, mesh.Vertices.Count);
		TestAssert.Equal(1f, mesh.Vertices[0].Y);
		TestAssert.Equal(3f, mesh.Vertices[1].Y);
		TestAssert.Equal(2f, mesh.Vertices[2].Y);
		TestAssert.Equal(4f, mesh.Vertices[3].Y);
	}

	private static void MeshTopologyMatchesGrid()
	{
		var system = CreateSystem();
		var heightmap = system.Generator.Generate(seed: 1, width: 4, height: 3, DefaultConfig());
		var mesh = system.MeshBuilder.Build(heightmap, DefaultConfig());

		TestAssert.Equal(12, mesh.Vertices.Count);
		TestAssert.Equal(12, mesh.Normals.Count);
		TestAssert.Equal(36, mesh.Indices.Count);
	}

	private static void FlatHeightmapNormalsPointUp()
	{
		var system = CreateSystem();
		var heightmap = Heightmap.FromSamples(new[,]
		{
			{ 5f, 5f, 5f },
			{ 5f, 5f, 5f },
			{ 5f, 5f, 5f }
		});

		var mesh = system.MeshBuilder.Build(heightmap, new TerrainMeshConfig());

		foreach (var normal in mesh.Normals)
		{
			TestAssert.True(MathF.Abs(normal.X) < 0.01f);
			TestAssert.True(normal.Y > 0.99f);
			TestAssert.True(MathF.Abs(normal.Z) < 0.01f);
		}
	}

	private static void MeshVerticesGoldenHash()
	{
		var system = CreateSystem();
		var config = new TerrainMeshConfig
		{
			CellSize = 1f,
			HeightScale = 2f,
			MinHeight = 0f,
			MaxHeight = 8f,
			NoiseFrequency = 0.12f,
			NoiseOctaves = 2
		};

		var heightmap = system.Generator.Generate(seed: 99, width: 5, height: 5, config);
		var mesh = system.MeshBuilder.Build(heightmap, config);

		const string expectedHash = "33AF7EBCC567F60E59AAECAE5C4F281C031D113A4A6519191E5DFECD22B7296E";
		var actualHash = ComputeVertexHash(mesh);
		TestAssert.Equal(expectedHash, actualHash);
	}

	private static string ComputeVertexHash(TerrainMeshData mesh)
	{
		var builder = new StringBuilder(mesh.Vertices.Count * 24);

		foreach (var vertex in mesh.Vertices)
			builder.Append(vertex.X.ToString("R"))
				.Append('|')
				.Append(vertex.Y.ToString("R"))
				.Append('|')
				.Append(vertex.Z.ToString("R"))
				.Append(';');

		var bytes = Encoding.UTF8.GetBytes(builder.ToString());
		var hash = SHA256.HashData(bytes);
		return Convert.ToHexString(hash);
	}
}
