using System;
using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Domain.AgentCommand.Core;
using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Adapter that consumes commands from AgentCommandSystem and applies movement to AgentMovementController.
/// This is a simple pull-based adapter: caller should call ExecutePendingCommands to transfer commands.
/// Now implements ITickable so the runtime can call Tick which executes and clears commands.
/// </summary>
public sealed class CommandToMovementAdapter : ITickable
{
    private readonly AgentCommandSystem _commandSystem;
    private readonly IAgentMovementController _movementController;
    private readonly Foundation.GameMath.Interfaces.IGameMath _math;

    public CommandToMovementAdapter(AgentCommandSystem commandSystem, IAgentMovementController movementController, Foundation.GameMath.Interfaces.IGameMath math)
    {
        _commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
        _movementController = movementController ?? throw new ArgumentNullException(nameof(movementController));
        _math = math ?? throw new ArgumentNullException(nameof(math));
    }

    /// <summary>
    /// Pulls pending commands and applies MoveCommand to the movement controller.
    /// After execution, this clears the command buffer.
    /// </summary>
    public void ExecutePendingCommands()
    {
        var cmds = _commandSystem.GetCommands();
        foreach (var c in cmds)
        {
            if (c is MoveCommand m)
            {
                var v = _math.Create(m.Direction.X, 0f, m.Direction.Y);
                // convert AgentId to EntityId mapping: here we assume AgentId.Value == EntityId.Value
                var entity = new EntityId(m.Agent.Value);
                _movementController.ApplyMovement(entity, v);
            }
        }

        // Clear after applying commands so next tick receives fresh buffer
        _commandSystem.ClearCommands();
    }

    // ITickable implementation: execute pending commands each tick
    public void Tick(float deltaTime)
    {
        ExecutePendingCommands();
    }
}
