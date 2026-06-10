using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Enemies;
using Game.Systems.Integration.Presentation;
using Game.Systems.Integration.World;

namespace Game.Tests.Integration.Runners;

public sealed class VitalityMonitorIntegrationResult
{
	public VitalityMonitorIntegrationResult(
		bool enemyRemovedFromRegistry,
		bool enemyRemovedFromMovement,
		bool enemyRemovedFromCombat,
		bool playerMarkedDead,
		bool ticksAfterEnemyDeathWithoutError)
	{
		EnemyRemovedFromRegistry = enemyRemovedFromRegistry;
		EnemyRemovedFromMovement = enemyRemovedFromMovement;
		EnemyRemovedFromCombat = enemyRemovedFromCombat;
		PlayerMarkedDead = playerMarkedDead;
		TicksAfterEnemyDeathWithoutError = ticksAfterEnemyDeathWithoutError;
	}

	public bool EnemyRemovedFromRegistry { get; }
	public bool EnemyRemovedFromMovement { get; }
	public bool EnemyRemovedFromCombat { get; }
	public bool PlayerMarkedDead { get; }
	public bool TicksAfterEnemyDeathWithoutError { get; }
}

public sealed class VitalityMonitorIntegrationRunner
{
	public VitalityMonitorIntegrationResult RunEnemyCleanup()
	{
		var defaultGeneration = GameSessionDefaults.Default.World.Generation;
		var config = new GameSessionConfig
		{
			World = new WorldConfig
			{
				Generation = new WorldGenerationConfig
				{
					Width = 24,
					Height = 24,
					Seed = 4242,
					FillProbability = defaultGeneration.FillProbability,
					CellularAutomataIterations = defaultGeneration.CellularAutomataIterations,
					MaxAttempts = defaultGeneration.MaxAttempts,
					WaterPoolAttempts = defaultGeneration.WaterPoolAttempts,
					WaterPoolMaxSize = defaultGeneration.WaterPoolMaxSize,
					EnableCeilingLayer = defaultGeneration.EnableCeilingLayer,
					MinWallBlobSize = defaultGeneration.MinWallBlobSize,
					MinCaveAreaSize = defaultGeneration.MinCaveAreaSize,
					MaxCaveAreaSize = defaultGeneration.MaxCaveAreaSize,
					MinCaveEntrances = defaultGeneration.MinCaveEntrances,
					MaxCaveEntrances = defaultGeneration.MaxCaveEntrances,
					MinEntranceWidth = defaultGeneration.MinEntranceWidth,
					MaxEntranceWidth = defaultGeneration.MaxEntranceWidth,
					MinEntranceDepth = defaultGeneration.MinEntranceDepth,
					MaxEntranceDepth = defaultGeneration.MaxEntranceDepth,
					MaxCaveCount = defaultGeneration.MaxCaveCount,
					MaxCavesPerBlob = defaultGeneration.MaxCavesPerBlob,
					ExtraWallStackChance = defaultGeneration.ExtraWallStackChance,
					ExtraWallStackClusterChance = defaultGeneration.ExtraWallStackClusterChance,
					ExtraWallStackGrowPasses = defaultGeneration.ExtraWallStackGrowPasses,
					StartCeilingClearanceRadius = defaultGeneration.StartCeilingClearanceRadius
				}
			},
			Enemies = new EnemySpawnConfig { MinPolarBearCount = 1, MaxPolarBearCount = 1 }
		};

		var session = new TerrainDemoSessionBuilder().Build(
			config,
			new TerrainDemoSessionBuildOptions { Presenter = new NullWorldPresenter() });

		var bear = session.PolarBearSetup?.Bears[0]
		           ?? throw new InvalidOperationException("Expected one polar bear.");

		var health = session.Resources.Registry.TryGetDefinition<IHealthResourceDefinition>(bear.EntityId)
		             ?? throw new InvalidOperationException("Bear has no health.");

		while (!health.IsDepleted)
			health.Decrease(health.CurrentAmount);

		for (var i = 0; i < 5; i++)
			session.Runtime.Tick(0.05f);

		var ticksWithoutError = true;
		try
		{
			for (var i = 0; i < 10; i++)
				session.Runtime.Tick(0.05f);
		}
		catch
		{
			ticksWithoutError = false;
		}

		var movementRemoved = false;
		try
		{
			session.Movement.Input.GetPosition(bear.EntityId);
		}
		catch (KeyNotFoundException)
		{
			movementRemoved = true;
		}

		return new VitalityMonitorIntegrationResult(
			!session.ActorRegistry.TryGetActor(bear.EntityId, out _),
			movementRemoved,
			!session.Combat.Registry.TryGet(bear.EntityId, out _),
			false,
			ticksWithoutError);
	}

	public bool RunPlayerDeath()
	{
		var session = new TerrainDemoSessionBuilder().Build(
			GameSessionDefaults.Default,
			new TerrainDemoSessionBuildOptions { Presenter = new NullWorldPresenter() });

		var health = session.Resources.Registry.TryGetDefinition<IHealthResourceDefinition>(
			             session.Player.Player.EntityId)
		             ?? throw new InvalidOperationException("Player has no health.");

		while (!health.IsDepleted)
			health.Decrease(health.CurrentAmount);

		for (var i = 0; i < 5; i++)
			session.Runtime.Tick(0.05f);

		return session.SessionState.PlayerIsDead;
	}
}
