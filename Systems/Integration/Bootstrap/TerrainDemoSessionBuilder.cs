using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.Navigation.Controller;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Player;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Systems.Integration.Bootstrap;

public sealed class TerrainDemoSessionBuildOptions
{
	public GeneratedWorldMap? Map { get; init; }
	public TerrainBuildResult? Terrain { get; init; }
	public IInputSource? InputSource { get; init; }
	public IWorldPresenter? Presenter { get; init; }
	public IAgentFacingProvider? FacingProvider { get; init; }
	public Func<EntityId, IAgentFacingProvider>? FacingProviderFactory { get; init; }
	public ArcAttackAbilityDefinition? PlayerAttackAbility { get; init; }
}

public sealed class TerrainDemoSessionResult
{
	public required GameSessionConfig Config { get; init; }
	public required GeneratedWorldMap Map { get; init; }
	public required TerrainBuildResult Terrain { get; init; }
	public required InMemoryWorldDataSource WorldData { get; init; }
	public required GameRuntime Runtime { get; init; }
	public required PlayerAgentHandle Player { get; init; }
	public required ActorRegistry ActorRegistry { get; init; }
	public required AgentMovementSystem Movement { get; init; }
	public required AgentCommandSystem CommandSystem { get; init; }
	public required EntityResourceSystem Resources { get; init; }
	public required AgentCombatSystem Combat { get; init; }
	public required CombatRuntimeServices CombatServices { get; init; }
	public required GameSessionState SessionState { get; init; }
	public required AgentMovementStateAdapter MovementStateAdapter { get; init; }
	public required WalkableSurfaceHeightSampler SurfaceHeightSampler { get; init; }
	public required DefaultTileRulesProvider TileRules { get; init; }
	public PolarBearTerrainDemoSetupResult? PolarBearSetup { get; init; }
	public IActorLifecycleCleanup? VitalityCleanup { get; init; }
}

public sealed class TerrainDemoSessionBuilder
{
	public TerrainDemoSessionResult Build(
		GameSessionConfig config,
		TerrainDemoSessionBuildOptions? options = null)
	{
		if (config is null)
			throw new ArgumentNullException(nameof(config));

		options ??= new TerrainDemoSessionBuildOptions();

		var map = options.Map ?? WorldSessionGenerator.GenerateMap(config);
		var terrain = options.Terrain ?? WorldSessionGenerator.ComposeTerrain(config, map);
		var worldData = (InMemoryWorldDataSource)map.ToDataSource();
		var tileRules = new DefaultTileRulesProvider();
		var math = new GameMathSystem();
		var playerConfig = config.Player;
		var playerMovementConfig = playerConfig.ToMovementConfig();
		var tileMovementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementPolicy = new OccupancyAwareMovementPolicy(tileMovementPolicy);
		var movement = new AgentMovementSystem(math, movementPolicy, playerMovementConfig);
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var tileOccupancy = new MovementTileOccupancyQuery(actorRegistry, movement);
		movementPolicy.SetOccupancyQuery(tileOccupancy);

		var player = actorRegistry.RegisterActor(
			math.Create(map.Start.X + 0.5f, map.Start.Y + 0.5f, 0f),
			playerMovementConfig);

		var movementStateAdapter = new AgentMovementStateAdapter(
			actorRegistry,
			movement,
			tileRules,
			worldData);

		var navigationGrid = NavigationGridBuilder.Build(worldData, tileRules);
		var pathNavigator = new AgentPathNavigator(
			navigationGrid,
			new AStarGridPathfinder(),
			occupancy: tileOccupancy);

		var sessionState = new GameSessionState();
		var combatServices = new CombatRuntimeServices(
			new AgentOrientationStore(),
			new AttackCooldownTracker(),
			new CombatFeedbackStore(),
			sessionState);
		var resources = new EntityResourceSystem();
		var combat = new AgentCombatSystem(
			new CooldownRecordingAbilityExecutor(new AbilityExecutor(), combatServices));

		var playerHandle = new PlayerAgentFactory().Register(
			movement,
			combat,
			resources,
			combatServices,
			player,
			playerConfig,
			options.PlayerAttackAbility);

		var polarBearSetup = new PolarBearTerrainDemoSetup().TryBuild(
			map,
			config.Enemies,
			config.PolarBear,
			player,
			actorRegistry,
			math,
			movement,
			pathNavigator,
			combat,
			resources,
			combatServices);

		VitalityCleanupServices? vitalityCleanup = null;
		if (options.Presenter is not null)
		{
			vitalityCleanup = new VitalityCleanupServices(
				actorRegistry,
				movement,
				commandSystem,
				combat.Registry,
				options.Presenter,
				polarBearSetup?.BehaviourSystem.Behaviour);
		}

		var runtimeBuilder = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithExistingCombat(combat)
			.WithExistingResources(resources)
			.WithCombatRuntime(combatServices)
			.WithSessionState(sessionState)
			.WithPlayerEntity(player.EntityId)
			.WithExtraTickable(movementStateAdapter, StandardTickOrder.MovementState);

		if (options.InputSource is not null)
		{
			runtimeBuilder.WithInput(
				options.InputSource,
				player.AgentId,
				player.EntityId);
		}

		if (options.Presenter is not null)
			runtimeBuilder.WithPresenter(options.Presenter, actorRegistry, config.Terrain.WorldUnitsPerTile);

		if (options.FacingProviderFactory is not null)
			runtimeBuilder.WithFacingProvider(options.FacingProviderFactory(player.EntityId));
		else if (options.FacingProvider is not null)
			runtimeBuilder.WithFacingProvider(options.FacingProvider);

		if (vitalityCleanup is not null)
			runtimeBuilder.WithVitalityCleanup(vitalityCleanup);

		if (polarBearSetup is not null)
		{
			runtimeBuilder
				.WithBehaviour(polarBearSetup.BehaviourSystem)
				.WithExistingCognition(polarBearSetup.Cognition)
				.WithIntentAgents(polarBearSetup.BearAgentIds.ToArray())
				.WithFaceTargets(polarBearSetup.FaceTargetByEntity)
				.WithExtraTickable(polarBearSetup.PlayerPresence, StandardTickOrder.PreCognition);
		}

		var surfaceHeightSampler = new WalkableSurfaceHeightSampler(tileRules, worldData);

		return new TerrainDemoSessionResult
		{
			Config = config,
			Map = map,
			Terrain = terrain,
			WorldData = worldData,
			Runtime = runtimeBuilder.Build(),
			Player = playerHandle,
			ActorRegistry = actorRegistry,
			Movement = movement,
			CommandSystem = commandSystem,
			Resources = resources,
			Combat = combat,
			CombatServices = combatServices,
			SessionState = sessionState,
			MovementStateAdapter = movementStateAdapter,
			SurfaceHeightSampler = surfaceHeightSampler,
			TileRules = tileRules,
			PolarBearSetup = polarBearSetup,
			VitalityCleanup = vitalityCleanup
		};
	}
}
