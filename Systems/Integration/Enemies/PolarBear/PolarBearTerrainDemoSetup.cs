using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
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
using Game.Systems.Integration.Enemies.Common.Context;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Enemies.PolarBear;

public sealed class PolarBearTerrainDemoSetup
{
	public PolarBearTerrainDemoSetupResult? TryBuild(
		GeneratedWorldMap map,
		int minCount,
		int maxCount,
		ActorHandle player,
		ActorRegistry actorRegistry,
		IGameMath math,
		AgentMovementSystem movement,
		IAgentPathNavigator pathNavigator)
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

		var spawnTiles = PolarBearSpawnPlacer.Place(
			map.GroundLayer,
			map.Start,
			map.Goal,
			map.SeedUsed,
			minCount,
			maxCount);

		if (spawnTiles.Count == 0)
			return null;

		var bearConfig = new PolarBearConfig();
		var cognitionConfig = new WorldCognitionConfig
		{
			GridWidth = bearConfig.CognitionGridWidth,
			GridHeight = bearConfig.CognitionGridHeight,
			CellSize = bearConfig.CognitionCellSize,
			QueryRadiusCells = 2
		};

		var resources = new EntityResourceSystem();
		var cognition = new WorldCognitionSystem(cognitionConfig);
		var combat = new AgentCombatSystem(new AbilityExecutor());

		Vector2 GetPosition(EntityId entityId)
		{
			var pos = movement.Input.GetPosition(entityId);
			return new Vector2(pos.X, pos.Y);
		}

		var playerHealth = new HealthResource(player.EntityId, maximum: 100f);
		playerHealth.Attach(resources.Registry, player.EntityId);
		combat.Registry.Register(new CombatEntity(player.EntityId));

		var bears = new List<ActorHandle>(spawnTiles.Count);
		var bearAgentIds = new List<AgentId>(spawnTiles.Count);
		var contextProviders = new List<KeyValuePair<AgentId, IBehaviourContextProvider>>(spawnTiles.Count);
		var perceptions = new List<EcologicalTargetPerception>(spawnTiles.Count);

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
				GetPosition,
				cognition.Cognition,
				perception,
				bearConfig.ToPerceptionConfig(),
				bearConfig.ToTacticalConfig());

			bears.Add(bear);
			bearAgentIds.Add(bear.AgentId);
			perceptions.Add(perception);
			contextProviders.Add(new KeyValuePair<AgentId, IBehaviourContextProvider>(bear.AgentId, bearContext));
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
				pathNavigator);
		}

		var playerPresence = new PlayerPresenceAdapter(
			cognition.Cognition,
			player.EntityId,
			GetPosition,
			sprinting: true);

		return new PolarBearTerrainDemoSetupResult(
			spawnTiles,
			bears,
			bearAgentIds,
			behaviourSystem,
			combat,
			resources,
			cognition,
			playerPresence);
	}
}

public sealed class PolarBearTerrainDemoSetupResult
{
	public PolarBearTerrainDemoSetupResult(
		IReadOnlyList<WorldPosition> spawnTiles,
		IReadOnlyList<ActorHandle> bears,
		IReadOnlyList<AgentId> bearAgentIds,
		AgentBehaviourSystem behaviourSystem,
		AgentCombatSystem combat,
		EntityResourceSystem resources,
		WorldCognitionSystem cognition,
		ITickable playerPresence)
	{
		SpawnTiles = spawnTiles;
		Bears = bears;
		BearAgentIds = bearAgentIds;
		BehaviourSystem = behaviourSystem;
		Combat = combat;
		Resources = resources;
		Cognition = cognition;
		PlayerPresence = playerPresence;
	}

	public IReadOnlyList<WorldPosition> SpawnTiles { get; }
	public IReadOnlyList<ActorHandle> Bears { get; }
	public IReadOnlyList<AgentId> BearAgentIds { get; }
	public AgentBehaviourSystem BehaviourSystem { get; }
	public AgentCombatSystem Combat { get; }
	public EntityResourceSystem Resources { get; }
	public WorldCognitionSystem Cognition { get; }
	public ITickable PlayerPresence { get; }
}
