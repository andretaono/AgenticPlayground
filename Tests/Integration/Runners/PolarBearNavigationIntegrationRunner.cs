using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.Navigation.Controller;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.Common.Context;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Player;
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime;
using Game.Tests.Integration.Fixtures;

namespace Game.Tests.Integration.Runners;

public sealed class PolarBearNavigationIntegrationRunner
{
	private const float DeltaTime = 1f / 20f;
	private const int MaxTicks = 600;

	public PolarBearNavigationIntegrationResult Run()
	{
		var tiles = CreateMap(
			"GGGGGGG",
			"GGGGGGG",
			"GGWGGGG",
			"GGWGGGG",
			"GGWGGGG",
			"GGGGGGG",
			"GGGGGGG");

		var worldData = new InMemoryWorldDataSource(tiles);
		var tileRules = new DefaultTileRulesProvider();
		var navigationGrid = NavigationGridBuilder.Build(worldData, tileRules);

		var bearConfig = IntegrationTestConfigs.PolarBearNavigationScenario();

		var cognitionConfig = new WorldCognitionConfig
		{
			GridWidth = 16,
			GridHeight = 16,
			CellSize = 1f,
			QueryRadiusCells = 2
		};

		var math = new GameMathSystem();
		var tileMovementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementPolicy = new OccupancyAwareMovementPolicy(tileMovementPolicy);
		var playerConfig = IntegrationTestConfigs.PlayerMovement();
		var movement = new AgentMovementSystem(
			math,
			movementPolicy,
			playerConfig.ToMovementConfig());
		var commandSystem = new Game.Systems.Domain.AgentCommand.AgentCommandSystem();
		var resources = new Game.Systems.Domain.EntityResource.EntityResourceSystem();
		var cognition = new WorldCognitionSystem(cognitionConfig);
		var combat = new Game.Systems.Domain.AgentCombat.AgentCombatSystem(new AbilityExecutor());
		var combatServices = new CombatRuntimeServices(
			new AgentOrientationStore(),
			new AttackCooldownTracker(),
			new CombatFeedbackStore(),
			new GameSessionState());

		var actorRegistry = new ActorRegistry(commandSystem, movement);
		movementPolicy.SetOccupancyQuery(new MovementTileOccupancyQuery(actorRegistry, movement));
		var tileOccupancy = new MovementTileOccupancyQuery(actorRegistry, movement);
		var pathNavigator = new AgentPathNavigator(
			navigationGrid,
			new AStarGridPathfinder(),
			occupancy: tileOccupancy);
		var player = actorRegistry.RegisterActor(
			math.Create(6.5f, 3.5f, 0f),
			playerConfig.ToMovementConfig());
		var bear = actorRegistry.RegisterActor(
			math.Create(0.5f, 3.5f, 0f),
			bearConfig.ToMovementConfig());

		var playerHealth = new HealthResource(player.EntityId, maximum: 100f);
		playerHealth.Attach(resources.Registry, player.EntityId);
		combat.Registry.Register(new CombatEntity(player.EntityId));

		var perception = new EcologicalTargetPerception();

		Vector2 GetPosition(Game.Systems.Foundation.Primitives.EntityId entityId)
		{
			var pos = movement.Input.GetPosition(entityId);
			return new Vector2(pos.X, pos.Y);
		}

		var bearContext = new TrackedTargetContextProvider(
			bear.AgentId,
			bear.EntityId,
			player.EntityId,
			GetPosition,
			cognition.Cognition,
			perception,
			bearConfig.ToPerceptionConfig(),
			ArcAttackAbilityDefinition.Default);

		var behaviourSystem = new AgentBehaviourSystem(bearContext, new IdleBehaviour());

		new PolarBearAgentFactory().Register(
			bear,
			player.EntityId,
			bearConfig,
			perception,
			behaviourSystem.Behaviour,
			cognition.Cognition,
			combat,
			resources,
			combatServices,
			pathNavigator,
			GetPosition);

		var playerPresence = new PlayerPresenceAdapter(
			cognition.Cognition,
			player.EntityId,
			GetPosition,
			sprinting: true);

		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithBehaviour(behaviourSystem)
			.WithExistingCombat(combat)
			.WithExistingResources(resources)
			.WithExistingCognition(cognition)
			.WithIntentAgents(bear.AgentId)
			.WithExtraTickable(playerPresence, StandardTickOrder.PreCognition)
			.Build();

		var initialDistance = Distance(GetPosition(bear.EntityId), GetPosition(player.EntityId));
		var minimumDistance = initialDistance;
		var reachedStalkRange = false;

		for (var tick = 0; tick < MaxTicks; tick++)
		{
			runtime.Tick(DeltaTime);

			var bearPosition = GetPosition(bear.EntityId);
			var playerPosition = GetPosition(player.EntityId);
			var distance = Distance(bearPosition, playerPosition);
			minimumDistance = MathF.Min(minimumDistance, distance);

			if (distance <= bearConfig.StalkMaxDistance)
				reachedStalkRange = true;
		}

		var finalDistance = Distance(GetPosition(bear.EntityId), GetPosition(player.EntityId));

		return new PolarBearNavigationIntegrationResult(
			InitialDistance: initialDistance,
			FinalDistance: finalDistance,
			MinimumDistance: minimumDistance,
			ReachedStalkRange: reachedStalkRange,
			TrackingDetected: perception.IsTracking);
	}

	private static TileId[,] CreateMap(params string[] rows)
	{
		var width = rows[0].Length;
		var height = rows.Length;
		var tiles = new TileId[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				tiles[x, y] = rows[y][x] switch
				{
					'G' => TileIds.Ground,
					'W' => TileIds.Wall,
					_ => TileIds.Water
				};
			}
		}

		return tiles;
	}

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}
}

public sealed record PolarBearNavigationIntegrationResult(
	float InitialDistance,
	float FinalDistance,
	float MinimumDistance,
	bool ReachedStalkRange,
	bool TrackingDetected);
