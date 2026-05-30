using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Model;

public sealed class BehaviourContext
{
	public AgentId Agent { get; init; }
	public Vector2 Position { get; init; }
	public EntityId? TargetEntity { get; init; }
	public Vector2 TargetDirection { get; init; }
	public bool TargetInAttackRange { get; init; }

	public bool HasTarget => TargetEntity.HasValue;
}
