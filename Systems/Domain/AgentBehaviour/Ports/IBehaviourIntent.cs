using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IBehaviourIntent
{
	AgentId Agent { get; }
}
