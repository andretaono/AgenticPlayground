using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Presentation.Ports;

public interface IAgentFacingProvider
{
	bool TryGetForward(EntityId entityId, out Vector2 forward);
}
