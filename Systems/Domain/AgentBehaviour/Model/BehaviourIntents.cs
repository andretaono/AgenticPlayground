using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Model;

public readonly struct MoveBehaviourIntent : Ports.IBehaviourIntent
{
	public AgentId Agent { get; init; }
	public Vector2 Direction { get; init; }

	public MoveBehaviourIntent(AgentId agent, Vector2 direction)
	{
		Agent = agent;
		Direction = direction;
	}
}

public readonly struct AttackBehaviourIntent : Ports.IBehaviourIntent
{
	public AgentId Agent { get; init; }

	public AttackBehaviourIntent(AgentId agent)
	{
		Agent = agent;
	}
}
