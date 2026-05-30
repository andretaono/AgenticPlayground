using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;

namespace Game.Systems.Domain.AgentBehaviour.Model.Behaviours;

public sealed class AttackBehaviour : IBehaviour
{
	public BehaviourId Id => new("attack");
	public int Priority { get; }

	public AttackBehaviour(int priority = 20) => Priority = priority;

	public bool CanExecute(BehaviourContext context) =>
		context.HasTarget && context.TargetInAttackRange;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context) =>
		new IBehaviourIntent[] { new AttackBehaviourIntent(context.Agent) };
}
