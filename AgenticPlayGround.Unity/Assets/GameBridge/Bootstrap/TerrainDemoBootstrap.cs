using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.TerrainMesh;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime;
using Game.Systems.Integration.TerrainMesh;
using Game.UnityBridge.Input;
using Game.UnityBridge.Presentation;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;
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
		[SerializeField] private int _waterPoolAttempts = 12;
		[SerializeField] private int _waterPoolMaxSize = 5;

		[Header("Terrain mesh")]
		[SerializeField] private float _worldUnitsPerTile = 1f;
		[SerializeField] private float _heightScale = 1f;
		[SerializeField] private float _groundHeight = 0f;
		[SerializeField] private float _wallHeight = 1f;
		[SerializeField] private float _waterHeight = -1f;
		[SerializeField] private float _bevelInset = 0.3f;
		[SerializeField] private int _bevelSegments = 4;

		[Header("Player")]
		[SerializeField] private float _groundSpeed = 4f;
		[SerializeField] private float _swimSpeed = 2.5f;
		[SerializeField] private float _characterHalfHeight = 0.5f;
		[SerializeField] private float _turnSpeedDegrees = 180f;

		[Header("Scene")]
		[SerializeField] private Material _terrainMaterial;
		[SerializeField] private Camera _camera;

		[Header("Over-shoulder camera")]
		[SerializeField] private float _cameraFollowDistance = 5f;
		[SerializeField] private float _cameraShoulderHeight = 2.2f;
		[SerializeField] private float _cameraShoulderOffset = 0.65f;
		[SerializeField] private float _cameraLookHeight = 1.4f;
		[SerializeField] private float _cameraLookAhead = 2f;
		[SerializeField] private float _cameraYawSmoothTime = 0.18f;
		[SerializeField] private float _cameraPositionSmoothTime = 0.12f;
		[SerializeField] private float _cameraRotationSmoothTime = 0.1f;

		private GameRuntime _runtime;
		private UnityWorldPresenter _worldPresenter;
		private OverShoulderCameraFollow _cameraFollow;
		private PlayerFacingController _playerFacing;
		private EntityId _playerEntityId;

		private void Awake()
		{
			var generationConfig = new WorldGenerationConfig
			{
				Width = _mapWidth,
				Height = _mapHeight,
				Seed = _seed,
				FillProbability = _fillProbability,
				CellularAutomataIterations = _cellularAutomataIterations,
				MaxAttempts = _maxAttempts,
				WaterPoolAttempts = _waterPoolAttempts,
				WaterPoolMaxSize = _waterPoolMaxSize
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
						HeightScale = _heightScale
					},
					ModifierSettings: new TileHeightModifierSettings
					{
						GroundHeight = _groundHeight,
						WallHeight = _wallHeight,
						WaterHeight = _waterHeight,
						BevelInset = _bevelInset,
						BevelSegments = _bevelSegments
					}));

			var terrainRoot = new GameObject("TerrainRoot").transform;
			terrainRoot.SetParent(transform, worldPositionStays: false);

			var material = _terrainMaterial != null ? _terrainMaterial : CreateDefaultMaterial();
			var terrainPresenter = new UnityTerrainPresenter(terrainRoot, material);
			terrainPresenter.SyncTerrainMesh(buildResult);

			var worldData = (InMemoryWorldDataSource)map.ToDataSource();
			var tileRules = new DefaultTileRulesProvider();
			var math = new GameMathSystem();
			var movement = new AgentMovementSystem(
				math,
				new AgentMovementPolicy(tileRules, worldData),
				new AgentMovementConfig(_groundSpeed, _swimSpeed, _groundSpeed));
			var commandSystem = new AgentCommandSystem();
			var actorRegistry = new ActorRegistry(commandSystem, movement);

			var startX = map.Start.X + 0.5f;
			var startY = map.Start.Y + 0.5f;
			var player = actorRegistry.RegisterActor(math.Create(startX, startY, 0f));
			_playerEntityId = player.EntityId;

			var actorsRoot = new GameObject("ActorsRoot").transform;
			actorsRoot.SetParent(transform, worldPositionStays: false);

			_worldPresenter = new UnityWorldPresenter(
				actorsRoot,
				buildResult.Heightmap,
				_worldUnitsPerTile,
				_heightScale,
				_characterHalfHeight,
				new TerrainMeshSystem().Sampler);

			_playerFacing = new PlayerFacingController();
			var inputSource = new UnityInputSource(player.AgentId, _playerFacing);
			var movementStateAdapter = new AgentMovementStateAdapter(
				actorRegistry,
				movement,
				tileRules,
				worldData);

			_runtime = new GameRuntimeBuilder(math)
				.WithExistingMovement(movement)
				.WithExistingCommand(commandSystem)
				.WithInput(inputSource, player.AgentId)
				.WithPresenter(_worldPresenter, actorRegistry)
				.WithExtraTickable(movementStateAdapter, StandardTickOrder.MovementState)
				.Build();

			var startPosition = movement.Input.GetPosition(player.EntityId);
			_worldPresenter.SyncActorPosition(
				player.EntityId,
				new GameVector2(startPosition.X, startPosition.Y));

			_cameraFollow = new OverShoulderCameraFollow
			{
				Settings = new OverShoulderCameraFollow.SettingsConfig
				{
					FollowDistance = _cameraFollowDistance,
					ShoulderHeight = _cameraShoulderHeight,
					ShoulderOffset = _cameraShoulderOffset,
					LookHeight = _cameraLookHeight,
					LookAhead = _cameraLookAhead,
					YawSmoothTime = _cameraYawSmoothTime,
					PositionSmoothTime = _cameraPositionSmoothTime,
					RotationSmoothTime = _cameraRotationSmoothTime
				}
			};

			InitializeCameraFollow();
			Debug.Log(
				$"Terrain demo ready. Seed={map.SeedUsed}, Start=({map.Start.X},{map.Start.Y}), " +
				$"Goal=({map.Goal.X},{map.Goal.Y}), Vertices={buildResult.Mesh.Vertices.Count}. " +
				"W/S move forward/back, A/D turn, camera follows behind.");
		}

		private void Update()
		{
			_playerFacing?.ApplyTurnInput(
				UnityEngine.Input.GetAxisRaw("Horizontal"),
				Time.deltaTime,
				_turnSpeedDegrees);
			_runtime?.Tick(Time.deltaTime);
		}

		private void LateUpdate()
		{
			if (_cameraFollow == null ||
			    _worldPresenter == null ||
			    !_worldPresenter.TryGetTransform(_playerEntityId, out var playerTransform))
			{
				return;
			}

			var camera = _camera != null ? _camera : Camera.main;
			if (camera == null)
				return;

			_cameraFollow.LateUpdate(
				playerTransform,
				camera,
				_playerFacing.FacingYawDegrees);
		}

		private void InitializeCameraFollow()
		{
			if (_cameraFollow == null ||
			    !_worldPresenter.TryGetTransform(_playerEntityId, out var playerTransform))
			{
				return;
			}

			var camera = _camera != null ? _camera : Camera.main;
			if (camera == null)
				return;

			_cameraFollow.SnapTo(playerTransform, camera);
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
