using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Interfaces;

public interface IAgentCommandSystem
{
    void SubmitCommand(IAgentCommand command);
    void ClearCommands();
    void RegisterAgent(AgentId agentId);
    void UnregisterAgent(AgentId agentId);
    bool HasCommands();
}
