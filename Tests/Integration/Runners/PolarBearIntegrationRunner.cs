using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;
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
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime;
using Game.Systems.Integration.Runtime.Interfaces;
using Game.Tests.Integration.Fixtures;

namespace Game.Tests.Integration.Runners;

public sealed class PolarBearIntegrationRunner
{
	private const float DeltaTime = 1f / 20f;
	private const int MaxTicks = 500;
	private const int PostAttackTicks = 80;
	private const float RestTriggerDistance = 70f;

	public PolarBearIntegrationResult Run()
	{
		var bearConfig = IntegrationTestConfigs.PolarBearBehaviourScenario();
		var playerMovementConfig = IntegrationTestConfigs.PlayerMovement().ToMovementConfig();
		var cognitionConfig = new WorldCognitionConfig
		{
			GridWidth = bearConfig.CognitionGridWidth,
			GridHeight = bearConfig.CognitionGridHeight,
			CellSize = bearConfig.CognitionCellSize,
			QueryRadiusCells = 2
		};

		var math = new GameMathSystem();
		var movement = new Game.Systems.Domain.AgentMovement.AgentMovementSystem(
			math,
			new PermissiveMovementPolicy());
		var commandSystem = new Game.Systems.Domain.AgentCommand.AgentCommandSystem();
		var resources = new Game.Systems.Domain.EntityResource.EntityResourceSystem();
		var cognition = new WorldCognitionSystem(cognitionConfig);
		var combat = new Game.Systems.Domain.AgentCombat.AgentCombatSystem(
			new LoggingAbilityExecutor(new AbilityExecutor()));

		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(
			math.Create(64f, 0f, 0f),
			playerMovementConfig);
		var bear = actorRegistry.RegisterActor(
			math.Create(220f, 0f, 0f),
			bearConfig.ToMovementConfig());

		var playerHealth = new HealthResource(player.EntityId, maximum: 100f);
		playerHealth.Attach(resources.Registry, player.EntityId);
		combat.Registry.Register(new CombatEntity(player.EntityId));
		var initialPlayerHealth = playerHealth.CurrentAmount;

		var perception = new EcologicalTargetPerception();
		var playerPhase = new PlayerPhaseState();

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
			bearConfig.ToTacticalConfig());

		var playerDirection = new Vector2(1f, 0f);
		var playerContextProvider = new PolarBearDemoContextProvider(
			bearContext,
			player.AgentId,
			() => new BehaviourContext
			{
				Agent = player.AgentId,
				Position = GetPosition(player.EntityId),
				TargetDirection = playerDirection,
				TargetInAttackRange = false
			});

		var behaviourSystem = new AgentBehaviourSystem(playerContextProvider, new IdleBehaviour());

		new PolarBearAgentFactory().Register(
			bear,
			player.EntityId,
			bearConfig,
			perception,
			behaviourSystem.Behaviour,
			cognition.Cognition,
			combat,
			resources,
			new StraightLineNavigator());

		behaviourSystem.Behaviour.AddBehaviour(
			player.AgentId,
			new ScriptedPlayerBehaviour(playerPhase, playerDirection));

		var playerCognition = new PlayerCognitionRecorder(
			cognition.Cognition,
			() => GetPosition(player.EntityId),
			playerPhase);

		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithBehaviour(behaviourSystem)
			.WithExistingCombat(combat)
			.WithExistingResources(resources)
			.WithExistingCognition(cognition)
			.WithIntentAgents(player.AgentId, bear.AgentId)
			.WithExtraTickable(playerCognition, StandardTickOrder.PreCognition)
			.Build();

		var restingPhaseStarted = false;
		var attackCommitted = false;
		var firstAttackTick = -1;
		var ticksAfterAttack = 0;
		var trackingDetected = false;
		var advantageWithoutLowHealth = false;
		var behaviourTrace = new List<string>();

		for (var tick = 1; tick <= MaxTicks; tick++)
		{
			var distanceBeforeTick = Distance(GetPosition(player.EntityId), GetPosition(bear.EntityId));

			if (perception.IsTracking)
				trackingDetected = true;

			if (perception.IsTracking && !restingPhaseStarted && distanceBeforeTick <= RestTriggerDistance)
			{
				playerPhase.BeginRest();
				restingPhaseStarted = true;
			}

			runtime.Tick(DeltaTime);

			var active = behaviourSystem.Output.GetActiveBehaviour(bear.AgentId)?.Id.ToString() ?? "none";
			if (behaviourTrace.Count == 0 || behaviourTrace[^1] != active)
				behaviourTrace.Add(active);

			var readiness = EvaluateAttackReadiness(
				cognition.Cognition,
				playerHealth,
				GetPosition(bear.EntityId),
				GetPosition(player.EntityId),
				bearConfig,
				Distance(GetPosition(player.EntityId), GetPosition(bear.EntityId)));

			if (active == "polar-bear-attack" && !attackCommitted)
			{
				attackCommitted = true;
				firstAttackTick = tick;
				if (!readiness.LowHealth && readiness.HasAdvantage)
					advantageWithoutLowHealth = true;
			}

			if (attackCommitted)
			{
				ticksAfterAttack++;
				if (ticksAfterAttack >= PostAttackTicks)
					break;
			}
		}

		return new PolarBearIntegrationResult(
			AttackCommitted: attackCommitted,
			FirstAttackTick: firstAttackTick,
			FinalPlayerHealth: playerHealth.CurrentAmount,
			InitialPlayerHealth: initialPlayerHealth,
			TrackingDetected: trackingDetected,
			AdvantageWithoutLowHealth: advantageWithoutLowHealth,
			BehaviourTrace: behaviourTrace);
	}

	private static AttackReadiness EvaluateAttackReadiness(
		IWorldCognitionController cognition,
		IHealthResourceDefinition playerHealth,
		Vector2 bearPosition,
		Vector2 playerPosition,
		PolarBearConfig config,
		float distance)
	{
		var inAttackRange = distance <= config.AttackRange;
		var lowHealth = playerHealth.CurrentAmount <= config.VulnerableHealthThreshold;
		var playerCell = cognition.GetCell(playerPosition);
		var highPresence = playerCell.Presence >= config.VulnerablePresenceThreshold;
		var awareness = cognition.GetAwareness(bearPosition);
		var awarenessTracked = awareness >= AwarenessState.Tracked;
		var hasAdvantage = lowHealth || highPresence || awarenessTracked;

		return new AttackReadiness(
			inAttackRange,
			lowHealth,
			highPresence,
			awarenessTracked,
			hasAdvantage,
			inAttackRange && hasAdvantage,
			distance,
			playerCell.Presence,
			awareness);
	}

	private static float Distance(Vector2 a, Vector2 b)
	{
		var dx = a.X - b.X;
		var dy = a.Y - b.Y;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	private readonly record struct AttackReadiness(
		bool InAttackRange,
		bool LowHealth,
		bool HighPresence,
		bool AwarenessTracked,
		bool HasAdvantage,
		bool AllConditionsMet,
		float Distance,
		float PlayerPresence,
		AwarenessState Awareness);

	private sealed class PlayerPhaseState
	{
		public string Mode { get; private set; } = "walk";

		public float PresenceContribution =>
			Mode == "rest"
				? WorldCognitionContributions.Presence.Resting
				: WorldCognitionContributions.Presence.Sprinting;

		public void BeginRest() => Mode = "rest";
	}

	private sealed class ScriptedPlayerBehaviour : IBehaviour
	{
		private readonly PlayerPhaseState _phase;
		private readonly Vector2 _walkDirection;

		public ScriptedPlayerBehaviour(PlayerPhaseState phase, Vector2 walkDirection)
		{
			_phase = phase;
			_walkDirection = walkDirection;
		}

		public BehaviourId Id => new("scripted-player");
		public int Priority => 10;

		public bool CanExecute(BehaviourContext context) => true;

		public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context)
		{
			if (_phase.Mode == "rest")
				return Array.Empty<IBehaviourIntent>();

			return new IBehaviourIntent[] { new MoveBehaviourIntent(context.Agent, _walkDirection) };
		}
	}

	private sealed class PlayerCognitionRecorder : ITickable
	{
		private readonly IWorldCognitionController _cognition;
		private readonly Func<Vector2> _getPlayerPosition;
		private readonly PlayerPhaseState _phase;

		public PlayerCognitionRecorder(
			IWorldCognitionController cognition,
			Func<Vector2> getPlayerPosition,
			PlayerPhaseState phase)
		{
			_cognition = cognition;
			_getPlayerPosition = getPlayerPosition;
			_phase = phase;
		}

		public void Tick(float deltaTime)
		{
			_ = deltaTime;
			_cognition.AddPresence(_getPlayerPosition(), _phase.PresenceContribution);
		}
	}
}
