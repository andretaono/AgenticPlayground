using System;
using System.Collections.Generic;
using Game.AgentMovement.Core.Model;
using Game.AgentMovement.Core.Simulation;
using Game.AgentMovement.Interfaces;
using Game.Foundation.GameMath.Core;
using Game.Foundation.GameMath.Interfaces;
using Game.Foundation.Primitives;

namespace Game.AgentMovement.Core;

/// <summary>
/// Use-case implementation for agent movement. Owns per-agent state and advances simulation time.
/// </summary>
public sealed class AgentMovementSystem : IAgentMovementSystem
{
    private readonly IGameMath _math;
    private readonly AgentMovementConfig _config;
    private readonly Dictionary<EntityId, AgentMovementAgentState> _agents = new();

    public AgentMovementSystem(IGameMath math, AgentMovementConfig? config = null)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _config = config ?? AgentMovementConfig.Default;
    }

    public void CreateAgent(EntityId entityId, IVector3 initialPosition)
    {
        if (!_math.IsFinite(initialPosition))
            throw new ArgumentOutOfRangeException(nameof(initialPosition), "Initial position must be finite.");

        if (_agents.ContainsKey(entityId))
            throw new InvalidOperationException($"Agent already exists for entity '{entityId}'.");

        _agents[entityId] = new AgentMovementAgentState
        {
            Position = _math.Create(initialPosition.X, initialPosition.Y, initialPosition.Z),
            Velocity = GameMathSystem.Zero,
            PendingInput = GameMathSystem.Zero,
            MovementState = AgentMovementState.Grounded
        };
    }

    public bool RemoveAgent(EntityId entityId) => _agents.Remove(entityId);

    public AgentMovementState GetMovementState(EntityId entityId) => Get(entityId).MovementState;

    public void SetMovementState(EntityId entityId, AgentMovementState state) => Get(entityId).MovementState = state;

    public IVector3 GetPosition(EntityId entityId) => Get(entityId).Position;

    public IVector3 GetVelocity(EntityId entityId) => Get(entityId).Velocity;

    public void ApplyMovement(EntityId entityId, IVector3 input)
    {
        if (!_math.IsFinite(input))
            throw new ArgumentOutOfRangeException(nameof(input), "Movement input must be finite.");

        var agent = Get(entityId);
        agent.PendingInput = _math.Create(input.X, input.Y, input.Z);
    }

    public void AdvanceSimulation(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

        foreach (var kvp in _agents)
        {
            var agent = kvp.Value;
            AgentMovementSimulator.Step(agent, deltaTime, _math, _config);

            if (!_math.IsFinite(agent.Position))
                throw new InvalidOperationException($"Movement produced a non-finite position for entity '{kvp.Key}'.");
        }
    }

    private AgentMovementAgentState Get(EntityId entityId)
    {
        if (!_agents.TryGetValue(entityId, out var agent))
            throw new KeyNotFoundException($"No movement agent registered for entity '{entityId}'.");
        return agent;
    }
}

