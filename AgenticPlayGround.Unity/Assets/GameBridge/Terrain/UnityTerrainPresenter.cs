using System;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.TerrainMesh;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public sealed class UnityTerrainPresenter : ITerrainPresenter
	{
		private readonly Transform _terrainRoot;
		private readonly ITileRulesProvider _tileRules;
		private readonly float _heightScale;

		public UnityTerrainPresenter(
			Transform terrainRoot,
			float heightScale = 1f,
			ITileRulesProvider tileRules = null)
		{
			_terrainRoot = terrainRoot ?? throw new ArgumentNullException(nameof(terrainRoot));
			_heightScale = heightScale;
			_tileRules = tileRules ?? new DefaultTileRulesProvider();
			CaveCeilingVisibility = new CaveCeilingVisibility();
		}

		public CaveCeilingVisibility CaveCeilingVisibility { get; }

		public void SyncTerrain(
			GeneratedWorldMap map,
			TerrainBuildResult result,
			TileHeightModifierSettings settings,
			TileSurfaceMeshSettings? surfaceSettings = null)
		{
			if (map is null)
				throw new ArgumentNullException(nameof(map));
			if (result is null)
				throw new ArgumentNullException(nameof(result));
			if (settings is null)
				throw new ArgumentNullException(nameof(settings));

			ClearExistingTiles();

			var surfaceMesh = result.SurfaceMesh;
			if (surfaceMesh is null)
			{
				surfaceMesh = new TileSurfaceComposer(_tileRules).Compose(
					map,
					new WorldTerrainMapping(
						Seed: map.SeedUsed,
						WorldUnitsPerTile: result.Heightmap.CellSize,
						TerrainConfig: new Game.Systems.Domain.TerrainMesh.Model.TerrainMeshConfig
						{
							HeightScale = _heightScale
						},
						ModifierSettings: settings,
						SurfaceSettings: surfaceSettings ?? new TileSurfaceMeshSettings()));
			}

			SyncSurfaceMeshes(surfaceMesh);
		}

		private void SyncSurfaceMeshes(TileSurfaceMeshResult surfaceMesh)
		{
			foreach (var group in surfaceMesh.Groups)
			{
				var meshObject = new GameObject(BuildMeshObjectName(group));
				meshObject.transform.SetParent(_terrainRoot, worldPositionStays: false);

				var meshFilter = meshObject.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = UnityMeshUpload.CreateMesh(group.Mesh, meshObject.name);

				var meshRenderer = meshObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = ResolveMaterial(group.Material);

				if (group.Material == SurfaceMaterialId.CeilingCap && group.CaveRegionId >= 0)
					CaveCeilingVisibility.Register(group.CaveRegionId, meshRenderer);
			}
		}

		private static string BuildMeshObjectName(TileSurfaceMeshGroup group)
		{
			if (group.Material == SurfaceMaterialId.CeilingCap && group.CaveRegionId >= 0)
				return $"CeilingCap_Region_{group.CaveRegionId}";

			return group.Material.ToString();
		}

		private static Material ResolveMaterial(SurfaceMaterialId material) =>
			material switch
			{
				SurfaceMaterialId.Water => TileVisualMaterials.GetWaterMaterial(),
				SurfaceMaterialId.Ground => TileVisualMaterials.GetGroundMaterial(),
				SurfaceMaterialId.CaveGround => TileVisualMaterials.GetCaveGroundMaterial(),
				SurfaceMaterialId.Wall => TileVisualMaterials.GetWallMaterial(),
				SurfaceMaterialId.CeilingStack => TileVisualMaterials.GetCeilingMaterial(),
				SurfaceMaterialId.CeilingCap => TileVisualMaterials.GetCeilingMaterial(),
				_ => TileVisualMaterials.GetGroundMaterial()
			};

		private void ClearExistingTiles()
		{
			for (var i = _terrainRoot.childCount - 1; i >= 0; i--)
				UnityEngine.Object.Destroy(_terrainRoot.GetChild(i).gameObject);
		}
	}
}
