using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Core;

public readonly struct MoveCommand : Game.Systems.Domain.AgentCommand.Interfaces.IAgentCommand
{
    public AgentId Agent { get; init; }
    public Vector2 Direction { get; init; }

    public MoveCommand(AgentId agent, Vector2 direction)
    {
        Agent = agent;
        Direction = direction;
    }
}

public readonly struct AttackCommand : Game.Systems.Domain.AgentCommand.Interfaces.IAgentCommand
{
    public AgentId Agent { get; init; }

    public AttackCommand(AgentId agent)
    {
        Agent = agent;
    }
}
