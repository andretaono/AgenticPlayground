using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.TerrainMesh;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Runtime;
using Game.Systems.Integration.TerrainMesh;
using Game.UnityBridge.Debug;
using Game.UnityBridge.Input;
using Game.UnityBridge.Presentation;
using Game.UnityBridge.Runtime;
using Game.UnityBridge.Terrain;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public static class TerrainDemoComposition
	{
		public static TerrainDemoContext Build(
			TerrainDemoSettings settings,
			Transform bootstrapRoot,
			Material terrainMaterial,
			Camera camera)
		{
			_ = terrainMaterial;

			var map = GenerateMap(settings);
			LogCaveGeneration(map.CaveCarveDiagnostic);
			GroundLayerTextDumper.LogToConsole(map);
			var modifierSettings = CreateModifierSettings(settings);
			var buildResult = ComposeTerrain(settings, map, modifierSettings);
			var resolvedCamera = camera != null ? camera : UnityEngine.Camera.main;

			var sessionRoot = new UnityEngine.GameObject("TerrainDemoSession").transform;
			sessionRoot.SetParent(bootstrapRoot, worldPositionStays: false);

			var terrainRoot = new UnityEngine.GameObject("TerrainRoot").transform;
			terrainRoot.SetParent(sessionRoot, worldPositionStays: false);
			var terrainPresenter = new UnityTerrainPresenter(terrainRoot, settings.HeightScale);
			terrainPresenter.SyncTerrain(map, buildResult, modifierSettings);

			var worldData = (InMemoryWorldDataSource)map.ToDataSource();
			var tileRules = new DefaultTileRulesProvider();
			var math = new GameMathSystem();
			var movement = new AgentMovementSystem(
				math,
				new AgentMovementPolicy(tileRules, worldData),
				new AgentMovementConfig(settings.GroundSpeed, settings.SwimSpeed, settings.GroundSpeed));
			var commandSystem = new AgentCommandSystem();
			var actorRegistry = new ActorRegistry(commandSystem, movement);

			var player = actorRegistry.RegisterActor(math.Create(
				map.Start.X + 0.5f,
				map.Start.Y + 0.5f,
				0f));

			var actorsRoot = new UnityEngine.GameObject("ActorsRoot").transform;
			actorsRoot.SetParent(sessionRoot, worldPositionStays: false);

			var worldPresenter = new UnityWorldPresenter(
				actorsRoot,
				buildResult.Heightmap,
				settings.WorldUnitsPerTile,
				settings.HeightScale,
				settings.CharacterHalfHeight,
				new TerrainMeshSystem().Sampler);

			var facing = new PlayerFacingController();
			var inputSource = new UnityInputSource(player.AgentId, facing);
			var movementStateAdapter = new AgentMovementStateAdapter(
				actorRegistry,
				movement,
				tileRules,
				worldData);

			var runtime = new GameRuntimeBuilder(math)
				.WithExistingMovement(movement)
				.WithExistingCommand(commandSystem)
				.WithInput(inputSource, player.AgentId)
				.WithPresenter(worldPresenter, actorRegistry)
				.WithExtraTickable(movementStateAdapter, StandardTickOrder.MovementState)
				.Build();

			var startPosition = movement.Input.GetPosition(player.EntityId);
			worldPresenter.SyncActorPosition(
				player.EntityId,
				new GameVector2(startPosition.X, startPosition.Y));

			var cameraFollow = new OverShoulderCameraFollow
			{
				Settings = new OverShoulderCameraFollow.SettingsConfig
				{
					FollowDistance = settings.CameraFollowDistance,
					ShoulderHeight = settings.CameraShoulderHeight,
					ShoulderOffset = settings.CameraShoulderOffset,
					LookHeight = settings.CameraLookHeight,
					LookAhead = settings.CameraLookAhead,
					YawSmoothTime = settings.CameraYawSmoothTime,
					PositionSmoothTime = settings.CameraPositionSmoothTime,
					RotationSmoothTime = settings.CameraRotationSmoothTime
				}
			};

			var context = new TerrainDemoContext(
				runtime,
				map,
				player,
				worldPresenter,
				facing,
				cameraFollow,
				resolvedCamera);

			SnapCamera(context);
			AttachHosts(sessionRoot.gameObject, context, settings, terrainRoot);

			var groundTiles = CountTiles(map.GroundLayer, TileIds.Ground);
			var wallTiles = CountTiles(map.GroundLayer, TileIds.Wall);
			var waterTiles = CountTiles(map.GroundLayer, TileIds.Water);
			UnityEngine.Debug.Log(
				$"Terrain demo ready. Seed={map.SeedUsed}, Start=({map.Start.X},{map.Start.Y}), " +
				$"Goal=({map.Goal.X},{map.Goal.Y}), Ground={groundTiles}, Wall={wallTiles}, Water={waterTiles}. " +
				"W/S move forward/back, A/D turn, camera follows behind. Key 1=ground tile overlay.");

			return context;
		}

		private static GeneratedWorldMap GenerateMap(TerrainDemoSettings settings)
		{
			var generationConfig = new WorldGenerationConfig
			{
				Width = settings.MapWidth,
				Height = settings.MapHeight,
				Seed = settings.Seed,
				FillProbability = settings.FillProbability,
				CellularAutomataIterations = settings.CellularAutomataIterations,
				MaxAttempts = settings.MaxAttempts,
				WaterPoolAttempts = settings.WaterPoolAttempts,
				WaterPoolMaxSize = settings.WaterPoolMaxSize,
				EnableCeilingLayer = settings.EnableCeilingLayer,
				MinWallBlobSize = settings.MinWallBlobSize,
				MinCaveAreaSize = settings.MinCaveAreaSize,
				MaxCaveAreaSize = settings.MaxCaveAreaSize,
				MinCaveEntrances = settings.MinCaveEntrances,
				MaxCaveEntrances = settings.MaxCaveEntrances,
				MinEntranceWidth = settings.MinEntranceWidth,
				MaxEntranceWidth = settings.MaxEntranceWidth,
				MinEntranceDepth = settings.MinEntranceDepth,
				MaxEntranceDepth = settings.MaxEntranceDepth,
				MaxCaveCount = settings.MaxCaveCount,
				ExtraWallStackChance = settings.ExtraWallStackChance,
				ExtraWallStackClusterChance = settings.ExtraWallStackClusterChance,
				ExtraWallStackGrowPasses = settings.ExtraWallStackGrowPasses,
				StartCeilingClearanceRadius = settings.StartCeilingClearanceRadius
			};

			return new WorldGenerationSystem().Generator.Generate(generationConfig);
		}

		private static void LogCaveGeneration(CaveCarveDiagnostic diagnostic)
		{
			UnityEngine.Debug.Log(
				$"[CaveGeneration] Attempted={diagnostic.AttemptedCount}, Created={diagnostic.CreatedCount}");

			for (var i = 0; i < diagnostic.Caves.Count; i++)
			{
				var cave = diagnostic.Caves[i];
				UnityEngine.Debug.Log(
					$"[CaveGeneration] Cave {i + 1}/{diagnostic.CreatedCount}: " +
					$"region={cave.RegionId}, size={cave.FloorSize}, " +
					$"outerEntrance=({cave.OutermostEntrance.X},{cave.OutermostEntrance.Y})");
			}
		}

		private static TileHeightModifierSettings CreateModifierSettings(TerrainDemoSettings settings) =>
			new()
			{
				GroundHeight = settings.GroundHeight,
				WallHeight = settings.WallHeight,
				WaterHeight = settings.WaterHeight
			};

		private static TerrainBuildResult ComposeTerrain(
			TerrainDemoSettings settings,
			GeneratedWorldMap map,
			TileHeightModifierSettings modifierSettings)
		{
			var composer = new TerrainComposer(new DefaultTileRulesProvider());

			return composer.Compose(
				map.ToDataSource(),
				new WorldTerrainMapping(
					Seed: map.SeedUsed,
					WorldUnitsPerTile: settings.WorldUnitsPerTile,
					TerrainConfig: new TerrainMeshConfig
					{
						HeightScale = settings.HeightScale
					},
					ModifierSettings: modifierSettings));
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

		private static void SnapCamera(TerrainDemoContext context)
		{
			if (context.Camera == null ||
			    !context.WorldPresenter.TryGetTransform(context.Player.EntityId, out var playerTransform))
			{
				return;
			}

			context.CameraFollow.SnapTo(playerTransform, context.Camera);
		}

		private static void AttachHosts(
			GameObject sessionObject,
			TerrainDemoContext context,
			TerrainDemoSettings settings,
			Transform terrainRoot)
		{
			sessionObject.AddComponent<GameLoopHost>().Initialize(context, settings.TurnSpeedDegrees);
			sessionObject.AddComponent<CameraFollowHost>().Initialize(context);

			if (settings.EnableLayerDebug)
				sessionObject.AddComponent<WorldLayerDebugHost>().Initialize(context, settings, terrainRoot);
		}
	}
}
