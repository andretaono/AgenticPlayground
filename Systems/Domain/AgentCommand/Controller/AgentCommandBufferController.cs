using Game.Systems.Domain.AgentCommand.Ports;

namespace Game.Systems.Domain.AgentCommand.Controller;

internal sealed class AgentCommandBufferController
{
	private readonly List<IAgentCommand> _buffer = new();

	public void Add(IAgentCommand command) => _buffer.Add(command);

	public void Clear() => _buffer.Clear();

	public bool HasCommands() => _buffer.Count > 0;

	public IReadOnlyList<IAgentCommand> GetCommands() => _buffer.AsReadOnly();
}
