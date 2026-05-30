using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Domain.AgentCommand.Ports;

namespace Game.Systems.Domain.AgentCommand.Model;

public readonly struct MoveCommand : IAgentCommand
{
	public AgentId Agent { get; init; }
	public Vector2 Direction { get; init; }

	public MoveCommand(AgentId agent, Vector2 direction)
	{
		Agent = agent;
		Direction = direction;
	}
}

public readonly struct AttackCommand : IAgentCommand
{
	public AgentId Agent { get; init; }

	public AttackCommand(AgentId agent)
	{
		Agent = agent;
	}
}
