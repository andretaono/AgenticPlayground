using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Actors;

public interface IActorRegistry
{
	IReadOnlyList<ActorHandle> Actors { get; }

	ActorHandle RegisterActor(IVector3 position);

	EntityId RegisterEntity(IVector3 position);

	bool TryGetActor(EntityId entityId, out ActorHandle handle);
}
