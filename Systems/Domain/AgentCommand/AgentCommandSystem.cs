using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentCommand.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand;

/// <summary>
/// Root orchestrator: validates agent ownership and delegates to registry/buffer controllers.
/// </summary>
public sealed class AgentCommandSystem : IAgentCommandSystem
{
	private readonly AgentCommandRegistryController _registry;
	private readonly AgentCommandBufferController _buffer;

	public AgentCommandSystem()
	{
		_registry = new AgentCommandRegistryController();
		_buffer = new AgentCommandBufferController();
	}

	public void SubmitCommand(IAgentCommand command)
	{
		if (command is null) throw new ArgumentNullException(nameof(command));

		switch (command)
		{
			case MoveCommand m:
				if (!_registry.IsRegistered(m.Agent))
					throw new InvalidOperationException("Agent not registered: " + m.Agent);
				break;
			case AttackCommand a:
				if (!_registry.IsRegistered(a.Agent))
					throw new InvalidOperationException("Agent not registered: " + a.Agent);
				break;
		}

		_buffer.Add(command);
	}

	public void ClearCommands() => _buffer.Clear();

	public void RegisterAgent(AgentId agentId) => _registry.RegisterAgent(agentId);

	public void UnregisterAgent(AgentId agentId) => _registry.UnregisterAgent(agentId);

	public bool HasCommands() => _buffer.HasCommands();

	public IReadOnlyList<IAgentCommand> GetCommands() => _buffer.GetCommands();
}
