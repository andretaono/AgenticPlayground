using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Integration.Enemies.Common.Advantage;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Common.Behaviours;

public sealed class AdvantageAttackBehaviour : IBehaviour
{
	private readonly IEntityResourceRegistry _resources;
	private readonly IWorldCognitionController _cognition;
	private readonly ITargetTrackingState _tracking;
	private readonly EnemyTacticalConfig _config;
	private readonly AttackAdvantageEvaluator _advantageEvaluator;

	public AdvantageAttackBehaviour(
		IEntityResourceRegistry resources,
		IWorldCognitionController cognition,
		ITargetTrackingState tracking,
		EnemyTacticalConfig config,
		AttackAdvantageEvaluator advantageEvaluator)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
		_tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
		_config = config ?? throw new ArgumentNullException(nameof(config));
		_advantageEvaluator = advantageEvaluator ?? throw new ArgumentNullException(nameof(advantageEvaluator));
	}

	public BehaviourId Id => new($"{_config.IdPrefix}-attack");
	public int Priority => _config.AttackPriority;

	public bool CanExecute(BehaviourContext context) =>
		context.HasTarget &&
		context.TargetInAttackRange &&
		TargetIsAlive(context) &&
		HasAdvantageousOpportunity(context);

	private bool TargetIsAlive(BehaviourContext context)
	{
		if (!context.TargetEntity.HasValue)
			return false;

		var health = _resources.TryGetDefinition<IHealthResourceDefinition>(context.TargetEntity.Value);
		if (health is null)
			return true;

		return !health.IsDepleted;
	}

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context) =>
		new IBehaviourIntent[] { new AttackBehaviourIntent(context.Agent, context.TargetEntity!.Value) };

	private bool HasAdvantageousOpportunity(BehaviourContext context)
	{
		var advantageContext = new AdvantageContext(
			context.TargetEntity!.Value,
			context.Position,
			_tracking.LastKnownTargetPosition,
			_cognition,
			_resources);

		return _advantageEvaluator.HasAdvantage(advantageContext);
	}
}
