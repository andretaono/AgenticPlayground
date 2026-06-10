using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies;
using Game.Systems.Integration.Enemies.Common.Context;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Enemies.PolarBear;

public sealed class PolarBearTerrainDemoSetup
{
	public PolarBearTerrainDemoSetupResult? TryBuild(
		GeneratedWorldMap map,
		EnemySpawnConfig spawnConfig,
		PolarBearConfig bearConfig,
		ActorHandle player,
		ActorRegistry actorRegistry,
		IGameMath math,
		AgentMovementSystem movement,
		IAgentPathNavigator pathNavigator,
		AgentCombatSystem combat,
		EntityResourceSystem resources,
		CombatRuntimeServices combatServices)
	{
		if (map is null)
			throw new ArgumentNullException(nameof(map));
		if (actorRegistry is null)
			throw new ArgumentNullException(nameof(actorRegistry));
		if (math is null)
			throw new ArgumentNullException(nameof(math));
		if (movement is null)
			throw new ArgumentNullException(nameof(movement));
		if (pathNavigator is null)
			throw new ArgumentNullException(nameof(pathNavigator));
		if (combat is null)
			throw new ArgumentNullException(nameof(combat));
		if (resources is null)
			throw new ArgumentNullException(nameof(resources));
		if (combatServices is null)
			throw new ArgumentNullException(nameof(combatServices));

		if (spawnConfig is null)
			throw new ArgumentNullException(nameof(spawnConfig));
		if (bearConfig is null)
			throw new ArgumentNullException(nameof(bearConfig));

		var spawnTiles = PolarBearSpawnPlacer.Place(
			map.GroundLayer,
			map.Start,
			map.Goal,
			map.SeedUsed,
			spawnConfig.MinPolarBearCount,
			spawnConfig.MaxPolarBearCount);

		if (spawnTiles.Count == 0)
			return null;

		var cognitionConfig = new WorldCognitionConfig
		{
			GridWidth = bearConfig.CognitionGridWidth,
			GridHeight = bearConfig.CognitionGridHeight,
			CellSize = bearConfig.CognitionCellSize,
			QueryRadiusCells = 2
		};

		var cognition = new WorldCognitionSystem(cognitionConfig);
		var getPosition = MovementPositionQuery.Create(movement);

		var bears = new List<ActorHandle>(spawnTiles.Count);
		var bearAgentIds = new List<AgentId>(spawnTiles.Count);
		var contextProviders = new List<KeyValuePair<AgentId, IBehaviourContextProvider>>(spawnTiles.Count);
		var perceptions = new List<EcologicalTargetPerception>(spawnTiles.Count);
		var faceTargetByEntity = new Dictionary<EntityId, EntityId>();

		foreach (var tile in spawnTiles)
		{
			var bear = actorRegistry.RegisterActor(
				math.Create(tile.X + 0.5f, tile.Y + 0.5f, 0f),
				bearConfig.ToMovementConfig());
			var perception = new EcologicalTargetPerception();
			var bearContext = new TrackedTargetContextProvider(
				bear.AgentId,
				bear.EntityId,
				player.EntityId,
				getPosition,
				cognition.Cognition,
				perception,
				bearConfig.ToPerceptionConfig(),
				ArcAttackAbilityDefinition.Default);

			bears.Add(bear);
			bearAgentIds.Add(bear.AgentId);
			perceptions.Add(perception);
			contextProviders.Add(new KeyValuePair<AgentId, IBehaviourContextProvider>(bear.AgentId, bearContext));
			faceTargetByEntity[bear.EntityId] = player.EntityId;
		}

		var behaviourSystem = new AgentBehaviourSystem(
			new CompositeBehaviourContextProvider(contextProviders),
			new IdleBehaviour());

		var factory = new PolarBearAgentFactory();
		for (var i = 0; i < bears.Count; i++)
		{
			factory.Register(
				bears[i],
				player.EntityId,
				bearConfig,
				perceptions[i],
				behaviourSystem.Behaviour,
				cognition.Cognition,
				combat,
				resources,
				combatServices,
				pathNavigator,
				getPosition);
		}

		var playerPresence = new PlayerPresenceAdapter(
			cognition.Cognition,
			player.EntityId,
			getPosition,
			sprinting: true);

		return new PolarBearTerrainDemoSetupResult(
			spawnTiles,
			bears,
			bearAgentIds,
			behaviourSystem,
			cognition,
			playerPresence,
			faceTargetByEntity);
	}
}

public sealed class PolarBearTerrainDemoSetupResult
{
	public PolarBearTerrainDemoSetupResult(
		IReadOnlyList<WorldPosition> spawnTiles,
		IReadOnlyList<ActorHandle> bears,
		IReadOnlyList<AgentId> bearAgentIds,
		AgentBehaviourSystem behaviourSystem,
		WorldCognitionSystem cognition,
		ITickable playerPresence,
		IReadOnlyDictionary<EntityId, EntityId> faceTargetByEntity)
	{
		SpawnTiles = spawnTiles;
		Bears = bears;
		BearAgentIds = bearAgentIds;
		BehaviourSystem = behaviourSystem;
		Cognition = cognition;
		PlayerPresence = playerPresence;
		FaceTargetByEntity = faceTargetByEntity;
	}

	public IReadOnlyList<WorldPosition> SpawnTiles { get; }
	public IReadOnlyList<ActorHandle> Bears { get; }
	public IReadOnlyList<AgentId> BearAgentIds { get; }
	public AgentBehaviourSystem BehaviourSystem { get; }
	public WorldCognitionSystem Cognition { get; }
	public ITickable PlayerPresence { get; }
	public IReadOnlyDictionary<EntityId, EntityId> FaceTargetByEntity { get; }
}
