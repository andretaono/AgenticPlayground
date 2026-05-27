using System;
using Game.AgentMovement.Core.Model;
using Game.AgentMovement.Interfaces;
using Game.Foundation.GameMath.Core;
using Game.Foundation.GameMath.Interfaces;

namespace Game.AgentMovement.Core.Simulation;

internal static class AgentMovementSimulator
{
    public static void Step(AgentMovementAgentState agent, float deltaTime, IGameMath math, AgentMovementConfig config)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (math == null) throw new ArgumentNullException(nameof(math));
        if (config == null) throw new ArgumentNullException(nameof(config));

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

