using Game.Foundation.Primitives;

namespace Game.AgentMovement.Model;

internal sealed class AgentMovementStateStore
{
    public Dictionary<EntityId, AgentMovementAgentState> Agents { get; } = new();

    public AgentMovementAgentState Get(EntityId entityId)
    {
        if (!Agents.TryGetValue(entityId, out var agent))
            throw new KeyNotFoundException($"No movement agent registered for entity '{entityId}'.");
        return agent;
    }
}

