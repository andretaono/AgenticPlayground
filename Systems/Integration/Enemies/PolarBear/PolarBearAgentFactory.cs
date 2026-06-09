using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCombat;
using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Enemies.Common.Assembly;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;

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
		CombatRuntimeServices combatServices,
		IAgentPathNavigator pathNavigator,
		Func<EntityId, Vector2> getPosition)
	{
		CombatEntityRegistrar.RegisterArcAttacker(
			combat,
			resources,
			combatServices,
			bear.EntityId,
			ArcAttackAbilityDefinition.Default,
			maxHealth: config.MaxHealth,
			getPosition);

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
