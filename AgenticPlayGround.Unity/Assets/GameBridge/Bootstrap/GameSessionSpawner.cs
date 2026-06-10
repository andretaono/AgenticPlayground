using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Presentation.Ports;
using Game.UnityBridge.Configs;
using Game.UnityBridge.Input;
using Game.UnityBridge.Presentation;
using Game.UnityBridge.Runtime;
using Game.UnityBridge.Terrain;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public static class GameSessionSpawner
	{
		public static GameSessionContext Spawn(Transform bootRoot, GameSessionProfileAsset profile) =>
			profile != null
				? Spawn(bootRoot, profile.ToSessionConfig(), profile.ToDebugSettings())
				: Spawn(bootRoot, null, null);

		public static GameSessionContext Spawn(
			Transform bootRoot,
			GameSessionConfig config = null,
			DebugInputSettings debug = null)
		{
			config ??= GameSessionDefaults.Default;
			debug ??= DebugInputSettings.Default;

			var sessionRoot = new GameObject("GameSession").transform;
			sessionRoot.SetParent(bootRoot, worldPositionStays: false);

			var terrainRoot = new GameObject("TerrainRoot").transform;
			terrainRoot.SetParent(sessionRoot, worldPositionStays: false);

			var actorsRoot = new GameObject("ActorsRoot").transform;
			actorsRoot.SetParent(sessionRoot, worldPositionStays: false);

			var map = WorldSessionGenerator.GenerateMap(config);
			var terrain = WorldSessionGenerator.ComposeTerrain(config, map);
			var worldData = (InMemoryWorldDataSource)map.ToDataSource();
			var tileRules = new DefaultTileRulesProvider();
			var surfaceHeightSampler = new WalkableSurfaceHeightSampler(tileRules, worldData);

			var facing = new PlayerFacingController();
			var inputSource = new UnityInputSource(facing);
			var worldPresenter = new UnityWorldPresenter(
				actorsRoot,
				terrain.Heightmap,
				config.Terrain.WorldUnitsPerTile,
				config.Terrain.HeightScale,
				config.Player.CharacterHalfHeight,
				config.Player.BodyRadius,
				surfaceHeightSampler);

			var session = new TerrainDemoSessionBuilder().Build(
				config,
				new TerrainDemoSessionBuildOptions
				{
					Map = map,
					Terrain = terrain,
					InputSource = inputSource,
					Presenter = worldPresenter,
					FacingProviderFactory = playerId => new PlayerFacingProvider(facing, playerId)
				});

			inputSource.Bind(session.Player.Player.AgentId);

			var terrainPresenter = new UnityTerrainPresenter(terrainRoot, config.Terrain.HeightScale);
			terrainPresenter.SyncTerrain(
				session.Map,
				session.Terrain,
				config.Terrain.Heights,
				config.Terrain.SurfaceMesh);

			ConfigureBearVisuals(session, config, worldPresenter);

			var startPosition = session.Movement.Input.GetPosition(session.Player.Player.EntityId);
			worldPresenter.SyncActorPosition(
				session.Player.Player.EntityId,
				new GameVector2(startPosition.X, startPosition.Y));

			var camera = SpawnCamera(sessionRoot);
			SpawnDirectionalLight(sessionRoot, new Vector3(50f, -160f, 0f));
			SpawnDirectionalLight(sessionRoot, new Vector3(50f, 30f, 0f));

			var cameraFollow = new TopDownRpgCameraFollow { Config = config.Camera };

			var context = new GameSessionContext(
				session,
				worldPresenter,
				terrainPresenter,
				facing,
				cameraFollow,
				camera,
				sessionRoot,
				terrainRoot,
				debug);

			SnapCamera(context);
			AttachHosts(sessionRoot.gameObject, context, terrainPresenter.CaveCeilingVisibility, debug);
			LogStartup(session);

			return context;
		}

		private static void ConfigureBearVisuals(
			TerrainDemoSessionResult session,
			GameSessionConfig config,
			UnityWorldPresenter worldPresenter)
		{
			if (session.PolarBearSetup is null)
				return;

			var bearMovement = config.PolarBear.ToMovementConfig();
			foreach (var bear in session.PolarBearSetup.Bears)
			{
				worldPresenter.ConfigureActorVisual(
					bear.EntityId,
					new ActorVisualDescriptor
					{
						BodyRadius = bearMovement.BodyRadius,
						VerticalScale = 1.2f,
						ColorR = 0.92f,
						ColorG = 0.94f,
						ColorB = 0.97f,
						IsPolarBear = true
					});
			}
		}

		private static Camera SpawnCamera(Transform parent)
		{
			var cameraObject = new GameObject("Main Camera");
			cameraObject.transform.SetParent(parent, worldPositionStays: false);
			var camera = cameraObject.AddComponent<Camera>();
			camera.tag = "MainCamera";
			camera.clearFlags = CameraClearFlags.Skybox;
			return camera;
		}

		private static void SpawnDirectionalLight(Transform parent, Vector3 rotation)
		{
			var lightObject = new GameObject("Directional Light");
			lightObject.transform.SetParent(parent, worldPositionStays: false);
			lightObject.transform.rotation = Quaternion.Euler(rotation);
			var light = lightObject.AddComponent<Light>();
			light.type = LightType.Directional;
			light.shadowStrength = 0.5f;
		}

		private static void SnapCamera(GameSessionContext context)
		{
			if (!context.WorldPresenter.TryGetTransform(context.Player.EntityId, out var playerTransform))
				return;

			context.CameraFollow.SnapTo(playerTransform, context.Camera, context.Facing.FacingYawDegrees);
		}

		private static void AttachHosts(
			GameObject sessionObject,
			GameSessionContext context,
			CaveCeilingVisibility caveCeilingVisibility,
			DebugInputSettings debug)
		{
			sessionObject.AddComponent<GameLoopHost>().Initialize(context);
			sessionObject.AddComponent<CameraFollowHost>().Initialize(context);
			sessionObject.AddComponent<GameOverHost>().Initialize(context);
			sessionObject.AddComponent<CaveCeilingVisibilityHost>()
				.Initialize(context, caveCeilingVisibility);

			if (debug.EnableLayerDebug)
				sessionObject.AddComponent<WorldLayerDebugHost>().Initialize(context, context.TerrainRoot);
		}

		private static void LogStartup(TerrainDemoSessionResult session)
		{
			var map = session.Map;
			var groundTiles = CountTiles(map.GroundLayer, TileIds.Ground);
			var wallTiles = CountTiles(map.GroundLayer, TileIds.Wall);
			var waterTiles = CountTiles(map.GroundLayer, TileIds.Water);
			var polarBearCount = session.PolarBearSetup?.Bears.Count ?? 0;

			Debug.Log(
				$"Game session ready. Seed={map.SeedUsed}, Start=({map.Start.X},{map.Start.Y}), " +
				$"Goal=({map.Goal.X},{map.Goal.Y}), Ground={groundTiles}, Wall={wallTiles}, Water={waterTiles}, " +
				$"PolarBears={polarBearCount}. " +
				"W/S move forward/back, A/D turn, Space or click to attack, top-down camera follows. Key 1=ground tile overlay.");
		}

		private static int CountTiles(TileId[,] tiles, TileId tileId)
		{
			var count = 0;
			for (var y = 0; y < tiles.GetLength(1); y++)
			for (var x = 0; x < tiles.GetLength(0); x++)
			{
				if (tiles[x, y] == tileId)
					count++;
			}

			return count;
		}
	}
}
