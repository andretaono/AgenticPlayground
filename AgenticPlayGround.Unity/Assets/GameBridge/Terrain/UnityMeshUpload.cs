using Game.Systems.Domain.TerrainMesh.Model;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public static class UnityMeshUpload
	{
		public static Mesh CreateMesh(TerrainMeshData meshData, string meshName)
		{
			if (meshData is null)
				throw new System.ArgumentNullException(nameof(meshData));

			var mesh = new Mesh { name = meshName };
			Upload(mesh, meshData);
			return mesh;
		}

		public static void Upload(Mesh mesh, TerrainMeshData meshData)
		{
			var vertices = new Vector3[meshData.Vertices.Count];
			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = meshData.Vertices[i];
				vertices[i] = new Vector3(vertex.X, vertex.Y, vertex.Z);
			}

			var normals = new Vector3[meshData.Normals.Count];
			for (var i = 0; i < normals.Length; i++)
			{
				var normal = meshData.Normals[i];
				normals[i] = new Vector3(normal.X, normal.Y, normal.Z);
			}

			var triangles = new int[meshData.Indices.Count];
			for (var i = 0; i < triangles.Length; i++)
				triangles[i] = meshData.Indices[i];

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
		}
	}
}
