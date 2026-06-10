using Game.Systems.Domain.AgentBehaviour;
using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Behaviours;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Player;
using Game.Systems.Integration.Resources;
using Game.Systems.Integration.Runtime;
using Game.Tests.Integration.Runners;

namespace Game.Tests.Integration.Runners;

public sealed class AgentCombatIntegrationRunner
{
	private const float AttackRange = 2f;
	private const float DeltaTime = 1f / 20f;
	private const float GroundSpeed = 5f;

	public AgentCombatIntegrationResult Run()
	{
		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var commandSystem = new Game.Systems.Domain.AgentCommand.AgentCommandSystem();
		var resources = new EntityResourceSystem();
		var combatServices = new CombatRuntimeServices(
			new AgentOrientationStore(),
			new AttackCooldownTracker(),
			new CombatFeedbackStore(),
			new GameSessionState());
		var combat = new AgentCombatSystem(
			new CooldownRecordingAbilityExecutor(new AbilityExecutor(), combatServices));

		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var attacker = actorRegistry.RegisterActor(math.Create(0f, 0f, 0f));
		var target = actorRegistry.RegisterEntity(math.Create(1.5f, 0f, 0f));

		var attackerConfig = new PlayerConfig { GroundSpeed = GroundSpeed, SwimSpeed = GroundSpeed };
		var attackAbility = new ArcAttackAbilityDefinition { BasePower = 25f };
		new PlayerAgentFactory().Register(
			movement,
			combat,
			resources,
			combatServices,
			attacker,
			attackerConfig,
			attackAbility);

		new HealthResource(target, 100f).Attach(resources.Registry, target);

		var targetHealth = resources.Registry.TryGetDefinition<IHealthResourceDefinition>(target)
		                   ?? throw new InvalidOperationException($"Target '{target}' has no health resource.");
		var initialHealth = targetHealth.CurrentAmount;

		combat.Registry.Register(new CombatEntity(target));

		var lastDistance = ComputeDistance(movement, attacker.EntityId, target);

		var contextProvider = new ScriptedContextProvider(attacker.AgentId, () =>
		{
			var attackerPos = movement.Input.GetPosition(attacker.EntityId);
			var targetPos = movement.Input.GetPosition(target);
			var delta = new Vector2(targetPos.X - attackerPos.X, targetPos.Y - attackerPos.Y);
			lastDistance = delta.Magnitude();

			return new BehaviourContext
			{
				Agent = attacker.AgentId,
				Position = new Vector2(attackerPos.X, attackerPos.Y),
				TargetEntity = target,
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
			.WithCombatRuntime(combatServices)
			.WithIntentAgents(attacker.AgentId)
			.Build();

		var initialDistance = lastDistance;
		var chaseTicks = (int)Math.Ceiling((lastDistance - AttackRange) / (GroundSpeed * DeltaTime));
		var totalTicks = chaseTicks + 8;

		for (var tick = 1; tick <= totalTicks; tick++)
		{
			if (lastDistance <= AttackRange + 0.01f)
			{
				combatServices.Orientation.SetForward(attacker.EntityId, new Vector2(1f, 0f));
				commandSystem.SubmitCommand(new AttackCommand(attacker.AgentId, CombatAttackSentinel.ArcAttack));
			}

			combatRuntime.Tick(DeltaTime);
		}

		return new AgentCombatIntegrationResult(
			TargetDamaged: targetHealth.CurrentAmount < initialHealth,
			InitialDistance: initialDistance,
			FinalDistance: lastDistance,
			FinalTargetHealth: targetHealth.CurrentAmount,
			InitialTargetHealth: initialHealth);
	}

	private static float ComputeDistance(AgentMovementSystem movement, EntityId attackerEntityId, EntityId targetId)
	{
		var attackerPos = movement.Input.GetPosition(attackerEntityId);
		var targetPos = movement.Input.GetPosition(targetId);
		var delta = new Vector2(targetPos.X - attackerPos.X, targetPos.Y - attackerPos.Y);
		return delta.Magnitude();
	}
}
