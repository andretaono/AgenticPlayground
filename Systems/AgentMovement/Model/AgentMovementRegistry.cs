using Game.AgentMovement.Interfaces;
using Game.Foundation.GameMath.Core;
using Game.Foundation.GameMath.Interfaces;
using Game.Foundation.Primitives;

namespace Game.AgentMovement.Model;

internal sealed class AgentMovementRegistry : IAgentMovementRegistry
{
    private readonly IGameMath _math;
    private readonly AgentMovementStateStore _store;

    public AgentMovementRegistry(IGameMath math, AgentMovementStateStore store)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public void CreateAgent(EntityId entityId, IVector3 initialPosition)
    {
        if (!_math.IsFinite(initialPosition))
            throw new ArgumentOutOfRangeException(nameof(initialPosition), "Initial position must be finite.");

        if (_store.Agents.ContainsKey(entityId))
            throw new InvalidOperationException($"Agent already exists for entity '{entityId}'.");

        _store.Agents[entityId] = new AgentMovementAgentState
        {
            Position = _math.Create(initialPosition.X, initialPosition.Y, initialPosition.Z),
            Velocity = GameMathSystem.Zero,
            PendingInput = GameMathSystem.Zero,
            MovementState = AgentMovementState.Grounded
        };
    }

    public bool RemoveAgent(EntityId entityId) => _store.Agents.Remove(entityId);
}

