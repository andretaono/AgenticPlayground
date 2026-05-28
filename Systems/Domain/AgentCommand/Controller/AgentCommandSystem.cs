using Game.Systems.Domain.AgentCommand.Core;
using Game.Systems.Domain.AgentCommand.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Controller;

/// <summary>
/// Core AgentCommandSystem: buffers immutable commands submitted by various producers.
/// Deterministic, engine-agnostic, no simulation performed here.
/// </summary>
public sealed class AgentCommandSystem : IAgentCommandSystem
{
    private readonly HashSet<AgentId> _agents = new();
    private readonly List<IAgentCommand> _buffer = new();

    public AgentCommandSystem() { }

    public void SubmitCommand(IAgentCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // Validate ownership for known command shapes (best-effort, but system is agent-agnostic)
        switch (command)
        {
            case MoveCommand m:
                if (!_agents.Contains(m.Agent)) throw new InvalidOperationException("Agent not registered: " + m.Agent);
                break;
            case AttackCommand a:
                if (!_agents.Contains(a.Agent)) throw new InvalidOperationException("Agent not registered: " + a.Agent);
                break;
            default:
                // unknown command types: accept but do not validate agent
                break;
        }

        // Commands are immutable structs; we only store references to interface typed instances
        _buffer.Add(command);
    }

    public void ClearCommands()
    {
        _buffer.Clear();
    }

    public void RegisterAgent(AgentId agentId) => _agents.Add(agentId);

    public void UnregisterAgent(AgentId agentId) => _agents.Remove(agentId);

    public bool HasCommands() => _buffer.Count > 0;

    // Expose a read-only view for consumers (simulation runtime will pull commands)
    public IReadOnlyList<IAgentCommand> GetCommands() => _buffer.AsReadOnly();
}
