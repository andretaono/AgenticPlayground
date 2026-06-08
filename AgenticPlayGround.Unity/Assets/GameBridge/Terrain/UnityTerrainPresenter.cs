using System;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.TerrainMesh;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public sealed class UnityTerrainPresenter : ITerrainPresenter
	{
		private readonly UnityMeshFactory _meshFactory;
		private readonly Transform _terrainRoot;
		private readonly Material _terrainMaterial;

		public UnityTerrainPresenter(
			Transform terrainRoot,
			Material terrainMaterial,
			UnityMeshFactory meshFactory = null)
		{
			_terrainRoot = terrainRoot ?? throw new ArgumentNullException(nameof(terrainRoot));
			_terrainMaterial = terrainMaterial ?? throw new ArgumentNullException(nameof(terrainMaterial));
			_meshFactory = meshFactory ?? new UnityMeshFactory();
		}

		public void SyncTerrainMesh(WorldTerrainBuildResult buildResult)
		{
			if (buildResult == null)
				throw new ArgumentNullException(nameof(buildResult));

			ClearExistingMeshes();

			var mesh = _meshFactory.CreateMesh(buildResult.Mesh, buildResult.TileOverlay);
			var terrainObject = new GameObject("TerrainMesh");
			terrainObject.transform.SetParent(_terrainRoot, worldPositionStays: false);

			var meshFilter = terrainObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;

			var meshRenderer = terrainObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = _terrainMaterial;
		}

		private void ClearExistingMeshes()
		{
			for (var i = _terrainRoot.childCount - 1; i >= 0; i--)
				UnityEngine.Object.Destroy(_terrainRoot.GetChild(i).gameObject);
		}
	}
}
