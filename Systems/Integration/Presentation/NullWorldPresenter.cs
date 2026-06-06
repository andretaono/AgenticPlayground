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
}
