using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime;

namespace Game.Tests.Integration.Runners;

public sealed class AgentCombatIntegrationRunner
{
	private const float AttackRange = 2f;
	private const float DeltaTime = 1f / 20f;
	private const float GroundSpeed = 5f;

	public AgentCombatIntegrationResult Run()
	{
		var math = new GameMathSystem();
		var movement = new Game.Systems.Domain.AgentMovement.AgentMovementSystem(
			math,
			new PermissiveMovementPolicy());
		var commandSystem = new Game.Systems.Domain.AgentCommand.AgentCommandSystem();
		var resources = new Game.Systems.Domain.EntityResource.EntityResourceSystem();
		var combat = new Game.Systems.Domain.AgentCombat.AgentCombatSystem(
			new LoggingAbilityExecutor(new AbilityExecutor()));

		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var attacker = actorRegistry.RegisterActor(math.Create(0f, 0f, 0f));
		var targetId = actorRegistry.RegisterEntity(math.Create(6f, 0f, 0f));

		AttachHealth(resources, attacker.EntityId);
		AttachHealth(resources, targetId);

		var attackerCombatEntity = new CombatEntity(attacker.EntityId);
		var targetCombatEntity = new CombatEntity(targetId);
		var targetHealth = resources.Registry.TryGetDefinition<IHealthResourceDefinition>(targetId)
			?? throw new InvalidOperationException($"Target '{targetId}' has no health resource.");
		var initialHealth = targetHealth.CurrentAmount;

		var meleeAbility = MeleeAttackAbilityFactory.Create(
			combat.Registry,
			resources.Registry,
			basePower: 25f);
		attackerCombatEntity.AddAbilityTrigger(new PendingTargetTrigger(meleeAbility, attackerCombatEntity));
		combat.Registry.Register(attackerCombatEntity);
		combat.Registry.Register(targetCombatEntity);

		var lastDistance = ComputeDistance(movement, attacker.EntityId, targetId);

		var contextProvider = new ScriptedContextProvider(attacker.AgentId, () =>
		{
			var attackerPos = movement.Input.GetPosition(attacker.EntityId);
			var targetPos = movement.Input.GetPosition(targetId);
			var delta = new Vector2(
				targetPos.X - attackerPos.X,
				targetPos.Y - attackerPos.Y);
			lastDistance = delta.Magnitude();

			return new BehaviourContext
			{
				Agent = attacker.AgentId,
				Position = new Vector2(attackerPos.X, attackerPos.Y),
				TargetEntity = targetId,
				TargetDirection = lastDistance <= 1e-6f ? Vector2.Zero : delta.Normalized(),
				TargetInAttackRange = lastDistance <= AttackRange
			};
		});

		var behaviourSystem = new AgentBehaviourSystem(contextProvider, new IdleBehaviour());
		behaviourSystem.Behaviour.AddBehaviour(attacker.AgentId, new ChaseBehaviour(priority: 10));
		behaviourSystem.Behaviour.AddBehaviour(attacker.AgentId, new AttackBehaviour(priority: 20));

		var combatRuntime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithBehaviour(behaviourSystem)
			.WithExistingCombat(combat)
			.WithExistingResources(resources)
			.WithIntentAgents(attacker.AgentId)
			.Build();

		var initialDistance = lastDistance;
		var chaseTicks = (int)Math.Ceiling((lastDistance - AttackRange) / (GroundSpeed * DeltaTime));
		var totalTicks = chaseTicks + 4;

		for (var tick = 1; tick <= totalTicks; tick++)
			combatRuntime.Tick(DeltaTime);

		return new AgentCombatIntegrationResult(
			TargetDamaged: targetHealth.CurrentAmount < initialHealth,
			InitialDistance: initialDistance,
			FinalDistance: lastDistance,
			FinalTargetHealth: targetHealth.CurrentAmount,
			InitialTargetHealth: initialHealth);
	}

	private static float ComputeDistance(
		Game.Systems.Domain.AgentMovement.AgentMovementSystem movement,
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

	private static void AttachHealth(
		Game.Systems.Domain.EntityResource.EntityResourceSystem resources,
		EntityId entityId,
		float maximum = 100f)
	{
		var health = new HealthResource(entityId, maximum);
		health.Attach(resources.Registry, entityId);
	}

}
