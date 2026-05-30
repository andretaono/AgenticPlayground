using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Ports;

public interface IAgentCommand
{
	AgentId Agent { get; }
}
