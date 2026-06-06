using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Presentation.Ports;

public interface IWorldPresenter
{
	void SyncActorPosition(EntityId entityId, Vector2 position);
}
