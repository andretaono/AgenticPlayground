using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Scenarios;

public sealed class AgentCombatDemo : IScenario
{
	public string Name => "agent-combat";

	public void Run()
	{
		Console.WriteLine("=== Agent Combat: chase, attack, apply damage ===");

		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var commandSystem = new AgentCommandSystem();
		var resources = new EntityResourceSystem();
		var combat = new AgentCombatSystem(new LoggingAbilityExecutor(new AbilityExecutor()));

		var attackerId = new AgentId(1);
		var targetId = new EntityId(2);
		var attackerEntityId = new EntityId(attackerId.Value);
		const float attackRange = 2f;
		var healthResourceId = new ResourceId("health");

		commandSystem.RegisterAgent(attackerId);
		movement.Registry.CreateAgent(attackerEntityId, math.Create(0f, 0f, 0f));
		movement.Registry.CreateAgent(targetId, math.Create(6f, 0f, 0f));

		RegisterHealth(resources, attackerEntityId, healthResourceId);
		RegisterHealth(resources, targetId, healthResourceId);

		var attackerCombatEntity = new CombatEntity(attackerEntityId);
		var targetCombatEntity = new CombatEntity(targetId);
		var meleeAbility = MeleeAttackAbilityFactory.Create(
			combat.Registry,
			resources.Resource,
			healthResourceId,
			basePower: 25f);
		attackerCombatEntity.AddAbilityTrigger(new PendingTargetTrigger(meleeAbility, attackerCombatEntity));
		combat.Registry.Register(attackerCombatEntity);
		combat.Registry.Register(targetCombatEntity);

		var lastDistance = ComputeDistance(movement, attackerEntityId, targetId);

		var contextProvider = new ScriptedContextProvider(attackerId, () =>
		{
			var attackerPos = movement.Input.GetPosition(attackerEntityId);
			var targetPos = movement.Input.GetPosition(targetId);
			var delta = new Vector2(
				targetPos.X - attackerPos.X,
				targetPos.Y - attackerPos.Y);
			lastDistance = delta.Magnitude();

			return new BehaviourContext
			{
				Agent = attackerId,
				Position = new Vector2(attackerPos.X, attackerPos.Y),
				TargetEntity = targetId,
				TargetDirection = lastDistance <= 1e-6f ? Vector2.Zero : delta.Normalized(),
				TargetInAttackRange = lastDistance <= attackRange
			};
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(attackerId, new ChaseBehaviour(priority: 10));
		behaviourSystem.Behaviour.AddBehaviour(attackerId, new AttackBehaviour(priority: 20));

		var intentAdapter = new BehaviourIntentToCommandAdapter(behaviourSystem.Output, commandSystem);
		var commandExecution = new AgentCommandExecutionAdapter(
			commandSystem,
			movement.Input,
			math,
			combat.Registry);

		var runtime = new RuntimeSystem(new SimpleSchedule(new[]
		{
			new TickEntry(new AgentBehaviourSimulationAdapter(behaviourSystem.Simulation), Order: 40),
			new TickEntry(new BehaviourIntentSubmissionAdapter(intentAdapter, new[] { attackerId }), Order: 50),
			new TickEntry(commandExecution, Order: 75),
			new TickEntry(new AgentCombatSimulationAdapter(combat.Simulation), Order: 80),
			new TickEntry(new AgentMovementSimulationAdapter(movement.Simulation), Order: 100),
			new TickEntry(new EntityResourceSimulationAdapter(resources.Simulation), Order: 110)
		}));

		Console.WriteLine($"Attack range: {attackRange:F1}");
		Console.WriteLine($"Initial distance: {lastDistance:F1}");
		PrintHealth(resources, targetId, healthResourceId, "Target health before");

		const float deltaTime = 1f / 20f;
		const float groundSpeed = 5f;
		var chaseTicks = (int)Math.Ceiling((lastDistance - attackRange) / (groundSpeed * deltaTime));
		var totalTicks = chaseTicks + 4;

		for (var tick = 1; tick <= totalTicks; tick++)
		{
			runtime.Tick(deltaTime);

			var pos = movement.Input.GetPosition(attackerEntityId);
			var health = resources.Resource.GetResource(targetId, healthResourceId);
			Console.WriteLine(
				$"\nTick {tick}: distance={lastDistance:F1}, pos=({pos.X:F2}, {pos.Y:F2}), active={DescribeActive(behaviourSystem, attackerId)}");
			Console.WriteLine($"Target health: {health.CurrentAmount:F1}/{health.MaximumAmount:F1}");
		}
	}

	private static float ComputeDistance(
		AgentMovementSystem movement,
		EntityId attackerEntityId,
		EntityId targetId)
	{
		var attackerPos = movement.Input.GetPosition(attackerEntityId);
		var targetPos = movement.Input.GetPosition(targetId);
		var delta = new Vector2(
			targetPos.X - attackerPos.X,
			targetPos.Y - attackerPos.Y);
		return delta.Magnitude();
	}

	private static void RegisterHealth(
		EntityResourceSystem resources,
		EntityId entityId,
		ResourceId healthResourceId)
	{
		resources.Registry.AddResource(entityId, new ResourceDefinition(
			ResourceId: healthResourceId,
			Name: "Health",
			MaximumAmount: 100f,
			RegenerationRate: 0f,
			DepletionRate: 0f,
			InitialAmount: 100f));
	}

	private static void PrintHealth(
		EntityResourceSystem resources,
		EntityId entityId,
		ResourceId healthResourceId,
		string label)
	{
		var snapshot = resources.Resource.GetResource(entityId, healthResourceId);
		Console.WriteLine($"{label}: {snapshot.CurrentAmount:F1}/{snapshot.MaximumAmount:F1}");
	}

	private static string DescribeActive(AgentBehaviourSystem behaviourSystem, AgentId agentId) =>
		behaviourSystem.Output.GetActiveBehaviour(agentId)?.Id.ToString() ?? "none";

	private sealed class ScriptedContextProvider : IBehaviourContextProvider
	{
		private readonly AgentId _agentId;
		private readonly Func<BehaviourContext> _factory;

		public ScriptedContextProvider(AgentId agentId, Func<BehaviourContext> factory)
		{
			_agentId = agentId;
			_factory = factory;
		}

		public BehaviourContext GetContext(AgentId agentId)
		{
			if (!agentId.Equals(_agentId))
				throw new KeyNotFoundException($"No scripted context for agent '{agentId}'.");

			return _factory();
		}
	}

	private sealed class SimpleSchedule : ITickSchedule
	{
		public IReadOnlyList<TickEntry> Entries { get; }

		public SimpleSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
	}
}
