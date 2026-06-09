using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Enemies.Common.Advantage;
using Game.Systems.Integration.Enemies.Common.Behaviours;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;
using Game.Systems.Integration.Navigation;

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
		IWorldCognitionController cognition,
		IAgentPathNavigator pathNavigator)
	{
		if (tracking is null) throw new ArgumentNullException(nameof(tracking));
		if (tacticalConfig is null) throw new ArgumentNullException(nameof(tacticalConfig));
		if (advantageRules is null) throw new ArgumentNullException(nameof(advantageRules));
		if (behaviourController is null) throw new ArgumentNullException(nameof(behaviourController));
		if (resources is null) throw new ArgumentNullException(nameof(resources));
		if (cognition is null) throw new ArgumentNullException(nameof(cognition));
		if (pathNavigator is null) throw new ArgumentNullException(nameof(pathNavigator));

		var advantageEvaluator = new AttackAdvantageEvaluator(advantageRules);

		behaviourController.AddBehaviour(agentId, new PatrolBehaviour(tacticalConfig));
		behaviourController.AddBehaviour(agentId, new TrackTargetBehaviour(tracking, tacticalConfig, pathNavigator));
		behaviourController.AddBehaviour(agentId, new StalkTargetBehaviour(tracking, tacticalConfig, pathNavigator));
		behaviourController.AddBehaviour(
			agentId,
			new AdvantageAttackBehaviour(resources, cognition, tracking, tacticalConfig, advantageEvaluator));
	}
}
