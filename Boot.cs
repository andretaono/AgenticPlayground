using System.Collections.Generic;
using Game.AgentMovement.Core;
using Game.Foundation.GameMath.Core;
using Game.Runtime.Core;
using Game.Runtime.Interfaces;

namespace Game;

/// <summary>
/// Composition root. Wires dependencies and defines runtime tick ordering.
/// </summary>
public static class Boot
{
    private sealed class TickSchedule : ITickSchedule
    {
        public IReadOnlyList<TickEntry> Entries { get; }

        public TickSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
    }

    private sealed class AgentMovementRuntimeAdapter : ITickable
    {
        private readonly Game.AgentMovement.Interfaces.IAgentMovementSystem _movement;

        public AgentMovementRuntimeAdapter(Game.AgentMovement.Interfaces.IAgentMovementSystem movement)
        {
            _movement = movement ?? throw new System.ArgumentNullException(nameof(movement));
        }

        public void Tick(float deltaTime) => _movement.AdvanceSimulation(deltaTime);
    }

    public static RuntimeSystem CreateRuntime()
    {
        // Foundation
        var math = new GameMathSystem();

        // Gameplay systems
        var movement = new AgentMovementSystem(math);
        var movementAdapter = new AgentMovementRuntimeAdapter(movement);

        // Ordered schedule (lower Order ticks earlier)
        var entries = new List<TickEntry>
        {
            new(movementAdapter, Order: 100),
        };

        return new RuntimeSystem(new TickSchedule(entries));
    }
}

