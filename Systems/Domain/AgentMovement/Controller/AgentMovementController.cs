using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Controller;

internal sealed class AgentMovementController : IAgentMovementController
{
    private readonly IGameMath _math;
    private readonly AgentMovementStateStore _store;

    public AgentMovementController(IGameMath math, AgentMovementStateStore store)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public AgentMovementState GetMovementState(EntityId entityId) => _store.Get(entityId).MovementState;

    public void SetMovementState(EntityId entityId, AgentMovementState state) => _store.Get(entityId).MovementState = state;

    public IVector3 GetPosition(EntityId entityId) => _store.Get(entityId).Position;

    public IVector3 GetVelocity(EntityId entityId) => _store.Get(entityId).Velocity;

    public void ApplyMovement(EntityId entityId, IVector3 input)
    {
        if (!_math.IsFinite(input))
            throw new ArgumentOutOfRangeException(nameof(input), "Movement input must be finite.");

        var agent = _store.Get(entityId);
        agent.PendingInput = _math.Create(input.X, input.Y, input.Z);
    }
}

