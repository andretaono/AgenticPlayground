using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Controller;

internal sealed class AgentCommandRegistryController
{
	private readonly HashSet<AgentId> _agents = new();

	public void RegisterAgent(AgentId agentId) => _agents.Add(agentId);

	public void UnregisterAgent(AgentId agentId) => _agents.Remove(agentId);

	public bool IsRegistered(AgentId agentId) => _agents.Contains(agentId);
}
