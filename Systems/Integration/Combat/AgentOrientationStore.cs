using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class AgentOrientationStore
{
	private readonly Dictionary<EntityId, Vector2> _forwardByEntity = new();

	public void SetForward(EntityId entityId, Vector2 forward)
	{
		if (forward.X * forward.X + forward.Y * forward.Y <= 1e-8f)
			return;

		_forwardByEntity[entityId] = forward.Normalized();
	}

	public Vector2 GetForward(EntityId entityId) =>
		_forwardByEntity.TryGetValue(entityId, out var forward) ? forward : new Vector2(0f, 1f);
}
