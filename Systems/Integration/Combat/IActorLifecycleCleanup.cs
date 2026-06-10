using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Presentation.Ports;

namespace Game.Systems.Integration.Combat;

public interface IActorLifecycleCleanup
{
	void RemoveDeadActor(ActorHandle actor);
}
