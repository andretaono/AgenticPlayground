using Game.Systems.Domain.TerrainMesh;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.TerrainMesh;
using GameVector3 = Game.Systems.Foundation.GameMath.Core.Model.Vector3;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Game.UnityBridge.Bootstrap
{
	public sealed class TerrainDemoBootstrap : MonoBehaviour
	{
		[Header("World generation")]
		[SerializeField] private int _mapWidth = 64;
		[SerializeField] private int _mapHeight = 48;
		[SerializeField] private int _seed = 42;
		[SerializeField] private float _fillProbability = 0.48f;
		[SerializeField] private int _cellularAutomataIterations = 5;
		[SerializeField] private int _maxAttempts = 50;

		[Header("Terrain mesh")]
		[SerializeField] private float _worldUnitsPerTile = 1f;
		[SerializeField] private float _minHeight = 0f;
		[SerializeField] private float _maxHeight = 10f;
		[SerializeField] private float _heightScale = 1f;
		[SerializeField] private float _noiseFrequency = 0.08f;
		[SerializeField] private int _noiseOctaves = 4;
		[SerializeField] private float _seaLevel = 0.5f;
		[SerializeField] private float _cliffHeight = 8f;

		[Header("Scene")]
		[SerializeField] private Material _terrainMaterial;
		[SerializeField] private Camera _camera;
		[SerializeField] private float _cameraHeight = 40f;
		[SerializeField] private float _cameraPitch = 55f;

		private void Awake()
		{
			var generationConfig = new WorldGenerationConfig
			{
				Width = _mapWidth,
				Height = _mapHeight,
				Seed = _seed,
				FillProbability = _fillProbability,
				CellularAutomataIterations = _cellularAutomataIterations,
				MaxAttempts = _maxAttempts
			};

			var map = new WorldGenerationSystem().Generator.Generate(generationConfig);

			var composer = new WorldTerrainMeshComposer(
				new TerrainMeshSystem(),
				new DefaultTileRulesProvider());

			var buildResult = composer.Compose(
				map.ToDataSource(),
				new WorldTerrainMapping(
					Seed: map.SeedUsed,
					WorldUnitsPerTile: _worldUnitsPerTile,
					TerrainConfig: new TerrainMeshConfig
					{
						MinHeight = _minHeight,
						MaxHeight = _maxHeight,
						HeightScale = _heightScale,
						NoiseFrequency = _noiseFrequency,
						NoiseOctaves = _noiseOctaves
					},
					ModifierSettings: new TileHeightModifierSettings
					{
						SeaLevel = _seaLevel,
						CliffHeight = _cliffHeight
					}));

			var terrainRoot = new GameObject("TerrainRoot").transform;
			terrainRoot.SetParent(transform, worldPositionStays: false);

			var material = _terrainMaterial != null ? _terrainMaterial : CreateDefaultMaterial();
			var presenter = new UnityTerrainPresenter(terrainRoot, material);
			presenter.SyncTerrainMesh(buildResult);

			FrameCamera(map.Width, map.Height);
			Debug.Log(
				$"Terrain demo ready. Seed={map.SeedUsed}, Start=({map.Start.X},{map.Start.Y}), " +
				$"Goal=({map.Goal.X},{map.Goal.Y}), Vertices={buildResult.Mesh.Vertices.Count}");
		}

		private void FrameCamera(int mapWidth, int mapHeight)
		{
			var camera = _camera != null ? _camera : Camera.main;
			if (camera == null)
				return;

			var centerX = mapWidth * _worldUnitsPerTile * 0.5f;
			var centerZ = mapHeight * _worldUnitsPerTile * 0.5f;
			var distance = Mathf.Max(mapWidth, mapHeight) * _worldUnitsPerTile * 0.75f;

			camera.transform.position = new Vector3(
				centerX,
				_cameraHeight,
				centerZ - distance);
			camera.transform.rotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
		}

		private static Material CreateDefaultMaterial()
		{
			var shader = Shader.Find("GameBridge/VertexColorUnlit");
			if (shader == null)
				shader = Shader.Find("Universal Render Pipeline/Lit");
			if (shader == null)
				shader = Shader.Find("Standard");

			return new Material(shader);
		}
	}

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

		private static Vector3 ToUnity(GameVector3 vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}

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
