using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;

namespace Game.Systems.Integration.Presentation;

public sealed class NullWorldPresenter : IWorldPresenter
{
	public void SyncActorPosition(EntityId entityId, Vector2 position)
	{
		_ = entityId;
		_ = position;
	}

	public void SyncActorHealth(EntityId entityId, float current, float maximum)
	{
		_ = entityId;
		_ = current;
		_ = maximum;
	}

	public void SyncActorFacing(EntityId entityId, float yawDegrees)
	{
		_ = entityId;
		_ = yawDegrees;
	}

	public void ShowAttackArc(EntityId entityId, float range, float arcDegrees, float durationSeconds)
	{
		_ = entityId;
		_ = range;
		_ = arcDegrees;
		_ = durationSeconds;
	}

	public void RemoveActor(EntityId entityId) => _ = entityId;

	public void ConfigureActorVisual(EntityId entityId, ActorVisualDescriptor descriptor)
	{
		_ = entityId;
		_ = descriptor;
	}
}
