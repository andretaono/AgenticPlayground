using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Interfaces;

public interface IAgentCommand {
	public AgentId Agent { get; }
}
