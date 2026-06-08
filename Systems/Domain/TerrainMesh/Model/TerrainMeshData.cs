using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Domain.TerrainMesh.Model;

public sealed class TerrainMeshData
{
	internal TerrainMeshData(
		IReadOnlyList<Vector3> vertices,
		IReadOnlyList<int> indices,
		IReadOnlyList<Vector3> normals)
	{
		Vertices = vertices;
		Indices = indices;
		Normals = normals;
	}

	public IReadOnlyList<Vector3> Vertices { get; }
	public IReadOnlyList<int> Indices { get; }
	public IReadOnlyList<Vector3> Normals { get; }
}
