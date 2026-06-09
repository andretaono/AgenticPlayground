using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.Common.Assembly;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;
using Game.Systems.Integration.Resources;

namespace Game.Systems.Integration.Enemies.PolarBear;

public sealed class PolarBearAgentFactory
{
	private readonly PredatorEnemyAssembler _assembler = new();

	public PolarBearAgentHandle Register(
		ActorHandle bear,
		EntityId playerEntityId,
		PolarBearConfig config,
		EcologicalTargetPerception perception,
		IBehaviourController behaviourController,
		IWorldCognitionController cognition,
		AgentCombatSystem combat,
		EntityResourceSystem resources,
		IAgentPathNavigator pathNavigator)
	{
		AttachHealth(resources, bear.EntityId, maximum: 150f);

		var bearCombatEntity = new CombatEntity(bear.EntityId);
		var meleeAbility = MeleeAttackAbilityFactory.Create(
			combat.Registry,
			resources.Registry,
			basePower: config.MeleeBasePower);
		bearCombatEntity.AddAbilityTrigger(new PendingTargetTrigger(meleeAbility, bearCombatEntity));
		combat.Registry.Register(bearCombatEntity);

		_assembler.RegisterPredatorPipeline(
			bear.AgentId,
			perception,
			config.ToTacticalConfig(),
			config.CreateAdvantageRules(),
			behaviourController,
			resources.Registry,
			cognition,
			pathNavigator);

		return new PolarBearAgentHandle(bear, playerEntityId, perception, config);
	}

	private static void AttachHealth(EntityResourceSystem resources, EntityId entityId, float maximum)
	{
		var health = new HealthResource(entityId, maximum);
		health.Attach(resources.Registry, entityId);
	}
}

public sealed class PolarBearAgentHandle
{
	public PolarBearAgentHandle(
		ActorHandle bear,
		EntityId playerEntityId,
		EcologicalTargetPerception perception,
		PolarBearConfig config)
	{
		Bear = bear;
		PlayerEntityId = playerEntityId;
		Perception = perception;
		Config = config;
	}

	public ActorHandle Bear { get; }
	public AgentId AgentId => Bear.AgentId;
	public EntityId EntityId => Bear.EntityId;
	public EntityId PlayerEntityId { get; }
	public EcologicalTargetPerception Perception { get; }
	public PolarBearConfig Config { get; }
}
