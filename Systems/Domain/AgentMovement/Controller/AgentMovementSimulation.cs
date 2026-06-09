using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Interfaces;

using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Controller;

internal class AgentMovementSimulation : IAgentMovementSimulation
{
    private readonly IGameMath _math;
    private readonly AgentMovementStateStore _store;
	private readonly IAgentMovementPolicy _movementPolicy;

    public AgentMovementSimulation(
		IGameMath math,
		AgentMovementStateStore store,
		IAgentMovementPolicy movementPolicy)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _store = store ?? throw new ArgumentNullException(nameof(store));
		_movementPolicy = movementPolicy ?? throw new ArgumentNullException(nameof(movementPolicy));
	}

    public void AdvanceSimulation(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

        foreach (var kvp in _store.Agents)
        {
            var agent = kvp.Value;
            Step(kvp.Key, agent, deltaTime, _math);

            if (!_math.IsFinite(agent.Position))
                throw new InvalidOperationException($"Movement produced a non-finite position for entity '{kvp.Key}'.");
        }
    }

    private void Step(EntityId entityId, AgentMovementAgentState agent, float deltaTime, IGameMath math)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (math == null) throw new ArgumentNullException(nameof(math));

        var config = agent.MovementConfig;
        if (config == null)
            throw new InvalidOperationException($"Movement config missing for entity '{entityId}'.");

        var speed = agent.MovementState switch
        {
            AgentMovementState.Grounded => config.GroundSpeed,
            AgentMovementState.Swimming => config.SwimSpeed,
            AgentMovementState.Airborne => config.AirSpeed,
            _ => config.GroundSpeed
        };

        var direction = math.Normalize(agent.PendingInput);
        if (math.MagnitudeSquared(direction) <= 0f)
        {
            agent.PendingInput = GameMathSystem.Zero;
            agent.Velocity = GameMathSystem.Zero;
            return;
        }

        var velocity = math.Scale(direction, speed);
        var proposedPosition = math.Add(agent.Position, math.Scale(velocity, deltaTime));

        if (!_movementPolicy.CanMoveTo(entityId, proposedPosition))
        {
            agent.PendingInput = GameMathSystem.Zero;
            agent.Velocity = GameMathSystem.Zero;
            return;
        }

        agent.Velocity = velocity;
        agent.Position = proposedPosition;

        // Input is treated as "per-frame intent" (caller sets every frame).
        agent.PendingInput = GameMathSystem.Zero;
    }
}

