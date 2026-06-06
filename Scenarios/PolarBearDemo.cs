using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.Common.Context;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Scenarios;

public sealed class PolarBearDemo : IScenario
{
	public string Name => "polar-bear";

	public void Run()
	{
		Console.WriteLine("=== Polar Bear: scent detection, stalk, and ambush ===");
		Console.WriteLine("Phase 1: Player walks east, leaving a sprint scent trail.");
		Console.WriteLine("Phase 2: Player rests when bear closes in (builds vulnerable presence).");
		Console.WriteLine("Phase 3: Bear enters attack range with advantage and strikes.\n");

		var bearConfig = new PolarBearConfig
		{
			ScentDetectionThreshold = 0.2f,
			StalkMinDistance = 2f,
			StalkMaxDistance = 48f
		};
		var cognitionConfig = new WorldCognitionConfig
		{
			GridWidth = bearConfig.CognitionGridWidth,
			GridHeight = bearConfig.CognitionGridHeight,
			CellSize = bearConfig.CognitionCellSize,
			QueryRadiusCells = 2
		};

		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var commandSystem = new AgentCommandSystem();
		var resources = new EntityResourceSystem();
		var cognition = new WorldCognitionSystem(cognitionConfig);
		var combat = new AgentCombatSystem(new LoggingAbilityExecutor(new AbilityExecutor()));

		var playerAgentId = new AgentId(1);
		var bearAgentId = new AgentId(2);
		var playerEntityId = new EntityId(playerAgentId.Value);
		var bearEntityId = new EntityId(bearAgentId.Value);

		commandSystem.RegisterAgent(playerAgentId);
		commandSystem.RegisterAgent(bearAgentId);
		movement.Registry.CreateAgent(playerEntityId, math.Create(64f, 0f, 0f));
		movement.Registry.CreateAgent(bearEntityId, math.Create(220f, 0f, 0f));

		var playerHealth = new HealthResource(playerEntityId, maximum: 100f);
		playerHealth.Attach(resources.Registry, playerEntityId);

		var perception = new EcologicalTargetPerception();
		var playerPhase = new PlayerPhaseState();

		Vector2 GetPosition(EntityId entityId)
		{
			var pos = movement.Input.GetPosition(entityId);
			return new Vector2(pos.X, pos.Y);
		}

		var bearContext = new TrackedTargetContextProvider(
			bearAgentId,
			bearEntityId,
			playerEntityId,
			GetPosition,
			cognition.Cognition,
			perception,
			bearConfig.ToPerceptionConfig(),
			bearConfig.ToTacticalConfig());

		var playerDirection = new Vector2(1f, 0f);
		var playerContextProvider = new PolarBearDemoContextProvider(
			bearContext,
			playerAgentId,
			() => new BehaviourContext
			{
				Agent = playerAgentId,
				Position = GetPosition(playerEntityId),
				TargetDirection = playerDirection,
				TargetInAttackRange = false
			});

		var behaviourSystem = new AgentBehaviourSystem(playerContextProvider, new IdleBehaviour());

		new PolarBearAgentFactory().Register(
			bearAgentId,
			bearEntityId,
			playerEntityId,
			bearConfig,
			perception,
			behaviourSystem.Behaviour,
			cognition.Cognition,
			combat,
			resources);

		behaviourSystem.Behaviour.AddBehaviour(
			playerAgentId,
			new ScriptedPlayerBehaviour(playerPhase, playerDirection));

		var intentAdapter = new BehaviourIntentToCommandAdapter(behaviourSystem.Output, commandSystem);
		var commandExecution = new AgentCommandExecutionAdapter(
			commandSystem,
			movement.Input,
			math,
			combat.Registry);
		var playerCognition = new PlayerCognitionRecorder(
			cognition.Cognition,
			() => GetPosition(playerEntityId),
			playerPhase);

		var runtime = new RuntimeSystem(new SimpleSchedule(new[]
		{
			new TickEntry(playerCognition, Order: 30),
			new TickEntry(new WorldCognitionSimulationAdapter(cognition.Simulation), Order: 35),
			new TickEntry(new AgentBehaviourSimulationAdapter(behaviourSystem.Simulation), Order: 40),
			new TickEntry(new BehaviourIntentSubmissionAdapter(intentAdapter, new[] { playerAgentId, bearAgentId }), Order: 50),
			new TickEntry(commandExecution, Order: 75),
			new TickEntry(new AgentCombatSimulationAdapter(combat.Simulation), Order: 80),
			new TickEntry(new AgentMovementSimulationAdapter(movement.Simulation), Order: 100),
			new TickEntry(new EntityResourceSimulationAdapter(resources.Simulation), Order: 110)
		}));

		Console.WriteLine($"Player start: {GetPosition(playerEntityId)}");
		Console.WriteLine($"Bear start: {GetPosition(bearEntityId)} (attack range {bearConfig.AttackRange:F1}m)");
		Console.WriteLine($"Initial tracking: {perception.IsTracking}\n");

		const float deltaTime = 1f / 20f;
		const int maxTicks = 500;
		const int postAttackTicks = 40;
		const float restTriggerDistance = 70f;

		var restingPhaseStarted = false;
		var attackCommitted = false;
		var firstAttackTick = -1;
		var ticksAfterAttack = 0;
		var trackingLogged = false;

		for (var tick = 1; tick <= maxTicks; tick++)
		{
			var distanceBeforeTick = Distance(GetPosition(playerEntityId), GetPosition(bearEntityId));

			if (perception.IsTracking && !trackingLogged)
			{
				trackingLogged = true;
				Console.WriteLine($"--- Tick {tick}: Bear picks up the scent trail ---");
			}

			if (perception.IsTracking && !restingPhaseStarted && distanceBeforeTick <= restTriggerDistance)
			{
				playerPhase.BeginRest();
				restingPhaseStarted = true;
				Console.WriteLine(
					$"\n--- Tick {tick}: Phase 2 - Player stops to rest at distance {distanceBeforeTick:F1}m ---");
				Console.WriteLine("Resting builds presence; bear continues closing in.\n");
			}

			runtime.Tick(deltaTime);

			var playerPos = GetPosition(playerEntityId);
			var bearPos = GetPosition(bearEntityId);
			var distance = Distance(playerPos, bearPos);
			var readiness = EvaluateAttackReadiness(
				cognition.Cognition,
				resources.Registry,
				playerHealth,
				bearPos,
				playerPos,
				perception,
				bearConfig,
				distance);

			var active = behaviourSystem.Output.GetActiveBehaviour(bearAgentId)?.Id.ToString() ?? "none";

			if (active == "polar-bear-attack" && !attackCommitted)
			{
				attackCommitted = true;
				firstAttackTick = tick;
				Console.WriteLine($"\n*** Tick {tick}: BEAR COMMITS TO ATTACK ***");
				PrintAttackReadiness(readiness);
				if (!readiness.LowHealth && readiness.HasAdvantage)
				{
					Console.WriteLine(
						"  Note: lowHealth=false is valid — advantage requires only one signal " +
						"(highPresence or awarenessTracked, not wounded prey).");
				}

				Console.WriteLine($"Bear behaviour: {active}");
				Console.WriteLine($"Player health: {playerHealth.CurrentAmount:F1}/{playerHealth.MaximumAmount:F1}\n");
			}

			if (ShouldLogSnapshot(tick, distance, readiness, attackCommitted, firstAttackTick))
			{
				Console.WriteLine(
					$"Tick {tick}: distance={distance:F1}, playerPhase={playerPhase.Mode}, bear={active}");
				PrintAttackReadiness(readiness);
				Console.WriteLine(
					$"  player=({playerPos.X:F1}, {playerPos.Y:F1}) bear=({bearPos.X:F1}, {bearPos.Y:F1}) " +
					$"health={playerHealth.CurrentAmount:F1}");
			}

			if (attackCommitted)
			{
				ticksAfterAttack++;
				if (ticksAfterAttack >= postAttackTicks)
					break;
			}
		}

		if (!attackCommitted)
		{
			Console.WriteLine("\nBear did not reach attack conditions before the demo time limit.");
			return;
		}

		Console.WriteLine(
			$"\nDemo complete: first attack at tick {firstAttackTick}, " +
			$"final player health {playerHealth.CurrentAmount:F1}/{playerHealth.MaximumAmount:F1}");
	}

	private static bool ShouldLogSnapshot(
		int tick,
		float distance,
		AttackReadiness readiness,
		bool attackCommitted,
		int firstAttackTick)
	{
		if (attackCommitted)
			return tick == firstAttackTick || tick == firstAttackTick + 10 || tick == firstAttackTick + 20;

		if (tick is 1 or 20 or 60 or 100)
			return true;

		if (readiness.InAttackRange || distance <= 20f)
			return tick % 10 == 0;

		return readiness.HasAdvantage && tick % 20 == 0;
	}

	private static AttackReadiness EvaluateAttackReadiness(
		IWorldCognitionController cognition,
		IEntityResourceRegistry resources,
		IHealthResourceDefinition playerHealth,
		Vector2 bearPosition,
		Vector2 playerPosition,
		EcologicalTargetPerception perception,
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

	private static void PrintAttackReadiness(AttackReadiness readiness)
	{
		Console.WriteLine(
			$"  attack ready: inRange={readiness.InAttackRange} ({readiness.Distance:F1}m <= needed), " +
			$"advantage={readiness.HasAdvantage}");
		Console.WriteLine(
			$"    lowHealth={readiness.LowHealth}, highPresence={readiness.HighPresence} " +
			$"(presence={readiness.PlayerPresence:F1}), awarenessTracked={readiness.AwarenessTracked} " +
			$"(awareness={readiness.Awareness})");
		Console.WriteLine(
			$"    allConditionsMet={readiness.AllConditionsMet} " +
			$"(advantage is OR: lowHealth | highPresence | awarenessTracked)");
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

	private sealed class SimpleSchedule : ITickSchedule
	{
		public IReadOnlyList<TickEntry> Entries { get; }

		public SimpleSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
	}
}
