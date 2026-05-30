using System;
using System.Collections.Generic;
using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Domain.AgentCommand.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Maps keyboard input (W/A/S/D) into MoveCommand submissions to the AgentCommandSystem.
/// This is a pure, engine-agnostic adapter: it exposes methods callers can invoke when input occurs.
/// It also implements ITickable so a runtime can poll input each tick.
/// </summary>
public sealed class InputToCommandAdapter : ITickable
{
    private readonly AgentCommandSystem _commandSystem;
    private readonly AgentId _agentId;

    public InputToCommandAdapter(AgentCommandSystem commandSystem, AgentId agentId)
    {
        _commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
        _agentId = agentId;
    }

    /// <summary>
    /// Call this for a single key press (W/A/S/D). Other keys are ignored.
    /// </summary>
    public void OnKey(ConsoleKey key)
    {
        var dir = key switch
        {
            ConsoleKey.W => new Vector2(0f, -1f),
            ConsoleKey.S => new Vector2(0f, 1f),
            ConsoleKey.A => new Vector2(-1f, 0f),
            ConsoleKey.D => new Vector2(1f, 0f),
            _ => Vector2.Zero
        };

        if (dir.Equals(Vector2.Zero)) return;

        SubmitMove(dir);
    }

    /// <summary>
    /// Call this when you have a set of pressed keys (e.g. W+D) to compute a combined direction.
    /// </summary>
    public void OnKeys(IEnumerable<ConsoleKey> keys)
    {
        float x = 0f, y = 0f;
        foreach (var k in keys)
        {
            switch (k)
            {
                case ConsoleKey.W: y -= 1f; break;
                case ConsoleKey.S: y += 1f; break;
                case ConsoleKey.A: x -= 1f; break;
                case ConsoleKey.D: x += 1f; break;
            }
        }

        var dir = new Vector2(x, y);
        if (dir.Equals(Vector2.Zero)) return;

        // normalize to avoid faster diagonal movement
        var normalized = dir.Magnitude() <= 1e-6f ? Vector2.Zero : dir.Normalized();
        if (normalized.Equals(Vector2.Zero)) return;

        SubmitMove(normalized);
    }

    private void SubmitMove(Vector2 direction)
    {
        // Commands are immutable; create and submit
        var cmd = new MoveCommand(_agentId, direction);
        _commandSystem.SubmitCommand(cmd);
    }

    // ITickable implementation: polls Console keys (non-blocking) and submits commands
    public void Tick(float deltaTime)
    {
        try
        {
            if (!Console.KeyAvailable) return;

            var keys = new HashSet<ConsoleKey>();
            while (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                keys.Add(keyInfo.Key);
            }

            if (keys.Count > 0)
                OnKeys(keys);
        }
        catch (InvalidOperationException)
        {
            // Console not available (e.g., running in non-interactive environment) ù ignore.
        }
    }
}
