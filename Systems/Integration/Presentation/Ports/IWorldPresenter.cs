using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Presentation.Ports;

public interface IWorldPresenter
{
	void SyncActorPosition(EntityId entityId, Vector2 position);
	void SyncActorHealth(EntityId entityId, float current, float maximum);
	void SyncActorFacing(EntityId entityId, float yawDegrees);
	void ShowAttackArc(EntityId entityId, float range, float arcDegrees, float durationSeconds);
	void RemoveActor(EntityId entityId);
}
