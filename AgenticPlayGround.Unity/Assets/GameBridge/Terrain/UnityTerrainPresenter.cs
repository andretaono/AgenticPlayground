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
		}

		public void SyncTerrain(
			GeneratedWorldMap map,
			TerrainBuildResult result,
			TileHeightModifierSettings settings)
		{
			if (map is null)
				throw new ArgumentNullException(nameof(map));
			if (result is null)
				throw new ArgumentNullException(nameof(result));
			if (settings is null)
				throw new ArgumentNullException(nameof(settings));

			ClearExistingTiles();

			var cellSize = result.Heightmap.CellSize;
			var cubeHeight = cellSize * _heightScale;

			for (var y = 0; y < map.Height; y++)
			for (var x = 0; x < map.Width; x++)
			{
				var centerX = (x + 0.5f) * cellSize;
				var centerZ = (y + 0.5f) * cellSize;
				PlaceWaterLevelCube(x, y, centerX, centerZ, cellSize, cubeHeight, settings);
			}

			for (var y = 0; y < map.Height; y++)
			for (var x = 0; x < map.Width; x++)
			{
				var tile = map.GroundLayer[x, y];
				var rules = _tileRules.GetRules(tile);

				if (rules.HasFlag(TileRules.Swimable))
					continue;

				var centerX = (x + 0.5f) * cellSize;
				var centerZ = (y + 0.5f) * cellSize;

				if (rules.HasFlag(TileRules.BlocksMovement))
				{
					PlaceGroundCube(x, y, centerX, centerZ, cellSize, cubeHeight, settings);
					var wallCenterY = settings.WallHeight * _heightScale * 0.5f;
					CreateTileCube(
						$"Wall_{x}_{y}",
						centerX,
						wallCenterY,
						centerZ,
						cellSize,
						cubeHeight,
						TileVisualMaterials.GetWallMaterial());
				}
				else
				{
					PlaceGroundCube(x, y, centerX, centerZ, cellSize, cubeHeight, settings);
				}
			}
		}

		private void PlaceGroundCube(
			int x,
			int y,
			float centerX,
			float centerZ,
			float cellSize,
			float cubeHeight,
			TileHeightModifierSettings settings)
		{
			var groundTopY = settings.GroundHeight * _heightScale;
			var groundCenterY = groundTopY - cubeHeight * 0.5f;
			CreateTileCube(
				$"Ground_{x}_{y}",
				centerX,
				groundCenterY,
				centerZ,
				cellSize,
				cubeHeight,
				TileVisualMaterials.GetGroundMaterial());
		}

		private void PlaceWaterLevelCube(
			int x,
			int y,
			float centerX,
			float centerZ,
			float cellSize,
			float cubeHeight,
			TileHeightModifierSettings settings)
		{
			var waterTopY = settings.WaterHeight * _heightScale;
			var waterCenterY = waterTopY - cubeHeight * 0.5f;
			CreateTileCube(
				$"WaterLevel_{x}_{y}",
				centerX,
				waterCenterY,
				centerZ,
				cellSize,
				cubeHeight,
				TileVisualMaterials.GetWaterMaterial());
		}

		private void CreateTileCube(
			string name,
			float centerX,
			float centerY,
			float centerZ,
			float cellSize,
			float cubeHeight,
			Material material)
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.name = name;
			cube.transform.SetParent(_terrainRoot, worldPositionStays: false);
			cube.transform.localPosition = new Vector3(centerX, centerY, centerZ);
			cube.transform.localScale = new Vector3(cellSize, cubeHeight, cellSize);

			var renderer = cube.GetComponent<Renderer>();
			if (renderer != null)
				renderer.sharedMaterial = material;

			var collider = cube.GetComponent<Collider>();
			if (collider != null)
				UnityEngine.Object.Destroy(collider);
		}

		private void ClearExistingTiles()
		{
			for (var i = _terrainRoot.childCount - 1; i >= 0; i--)
				UnityEngine.Object.Destroy(_terrainRoot.GetChild(i).gameObject);
		}
	}
}
