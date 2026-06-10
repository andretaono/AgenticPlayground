using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Navigation;

public static class MovementPositionQuery
{
	public static Func<EntityId, Vector2> Create(AgentMovementSystem movement)
	{
		if (movement is null)
			throw new ArgumentNullException(nameof(movement));

		return entityId =>
		{
			var position = movement.Input.GetPosition(entityId);
			return new Vector2(position.X, position.Y);
		};
	}
}
