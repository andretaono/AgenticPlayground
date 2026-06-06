using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Enemies.Common.Advantage;
using Game.Systems.Integration.Enemies.Common.Behaviours;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Common.Assembly;

public sealed class PredatorEnemyAssembler
{
	public void RegisterPredatorPipeline(
		AgentId agentId,
		ITargetTrackingState tracking,
		EnemyTacticalConfig tacticalConfig,
		IReadOnlyList<IAttackAdvantageRule> advantageRules,
		IBehaviourController behaviourController,
		IEntityResourceRegistry resources,
		IWorldCognitionController cognition)
	{
		ArgumentNullException.ThrowIfNull(tracking);
		ArgumentNullException.ThrowIfNull(tacticalConfig);
		ArgumentNullException.ThrowIfNull(advantageRules);
		ArgumentNullException.ThrowIfNull(behaviourController);
		ArgumentNullException.ThrowIfNull(resources);
		ArgumentNullException.ThrowIfNull(cognition);

		var advantageEvaluator = new AttackAdvantageEvaluator(advantageRules);

		behaviourController.AddBehaviour(agentId, new PatrolBehaviour(tacticalConfig));
		behaviourController.AddBehaviour(agentId, new TrackTargetBehaviour(tracking, tacticalConfig));
		behaviourController.AddBehaviour(agentId, new StalkTargetBehaviour(tracking, tacticalConfig));
		behaviourController.AddBehaviour(
			agentId,
			new AdvantageAttackBehaviour(resources, cognition, tracking, tacticalConfig, advantageEvaluator));
	}
}
