using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Integration.Behaviours;

public sealed class ChaseBehaviour : IBehaviour
{
	public BehaviourId Id => new("chase");
	public int Priority { get; }

	public ChaseBehaviour(int priority = 10) => Priority = priority;

	public bool CanExecute(BehaviourContext context) =>
		context.HasTarget && !context.TargetInAttackRange;

	public IReadOnlyList<IBehaviourIntent> Execute(BehaviourContext context)
	{
		var direction = context.TargetDirection.Magnitude() <= 1e-6f
			? Vector2.Zero
			: context.TargetDirection.Normalized();

		return new IBehaviourIntent[]
		{
			new MoveBehaviourIntent(context.Agent, direction)
		};
	}
}
