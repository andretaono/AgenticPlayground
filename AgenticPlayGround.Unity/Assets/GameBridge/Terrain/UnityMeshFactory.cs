using System;
using System.Collections.Generic;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;
using GameVector3 = Game.Systems.Foundation.GameMath.Core.Model.Vector3;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public sealed class UnityMeshFactory
	{
		public Mesh CreateMesh(TerrainMeshData meshData, IReadOnlyList<TileId> tileOverlay = null)
		{
			if (meshData == null)
				throw new ArgumentNullException(nameof(meshData));

			var vertexCount = meshData.Vertices.Count;
			var vertices = new Vector3[vertexCount];
			var normals = new Vector3[vertexCount];
			var colors = new Color[vertexCount];

			for (var i = 0; i < vertexCount; i++)
			{
				vertices[i] = ToUnity(meshData.Vertices[i]);
				normals[i] = ToUnity(meshData.Normals[i]);
				colors[i] = tileOverlay != null && i < tileOverlay.Count
					? TileColor(tileOverlay[i])
					: Color.white;
			}

			var indices = new int[meshData.Indices.Count];
			for (var i = 0; i < meshData.Indices.Count; i++)
				indices[i] = meshData.Indices[i];

			var mesh = new Mesh
			{
				name = "GeneratedTerrainMesh",
				vertices = vertices,
				normals = normals,
				colors = colors,
				triangles = indices
			};
			mesh.RecalculateBounds();
			return mesh;
		}

		private static Vector3 ToUnity(GameVector3 vector) =>
			new(vector.X, vector.Y, vector.Z);

		private static Color TileColor(TileId tileId)
		{
			switch (tileId.Id)
			{
				case "ground":
					return new Color(0.76f, 0.70f, 0.50f);
				case "water":
					return new Color(0.20f, 0.40f, 0.80f);
				case "wall":
					return new Color(0.40f, 0.40f, 0.45f);
				default:
					return Color.white;
			}
		}
	}
}
