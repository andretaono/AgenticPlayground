using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Domain.AgentMovement.Controller;

internal class AgentMovementSimulation : IAgentMovementSimulation
{
    private readonly IGameMath _math;
    private readonly AgentMovementConfig _config;
    private readonly AgentMovementStateStore _store;
	private readonly IAgentMovementPolicy _movementPolicy;

    public AgentMovementSimulation(
		IGameMath math, 
		AgentMovementConfig config, 
		AgentMovementStateStore store,
		IAgentMovementPolicy movementPolicy)
    {
        _math = math ?? throw new ArgumentNullException(nameof(math));
        _config = config ?? throw new ArgumentNullException(nameof(config));
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
            Step(agent, deltaTime, _math, _config);

            if (!_math.IsFinite(agent.Position))
                throw new InvalidOperationException($"Movement produced a non-finite position for entity '{kvp.Key}'.");
        }
    }

    private void Step(AgentMovementAgentState agent, float deltaTime, IGameMath math, AgentMovementConfig config)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (math == null) throw new ArgumentNullException(nameof(math));
        if (config == null) throw new ArgumentNullException(nameof(config));

		if (!_movementPolicy.CanMove(agent))
			return;

        var speed = agent.MovementState switch
        {
            AgentMovementState.Grounded => config.GroundSpeed,
            AgentMovementState.Swimming => config.SwimSpeed,
            AgentMovementState.Airborne => config.AirSpeed,
            _ => config.GroundSpeed
        };

        var direction = math.Normalize(agent.PendingInput);
        agent.Velocity = math.Scale(direction, speed);
        agent.Position = math.Add(agent.Position, math.Scale(agent.Velocity, deltaTime));

        // Input is treated as "per-frame intent" (caller sets every frame).
        agent.PendingInput = GameMathSystem.Zero;
    }
}

