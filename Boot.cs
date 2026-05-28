using Game.Systems.Domain.AgentCommand.Example;
using Game.Systems.Domain.AgentMovement.Controller;
using Game.Systems.Domain.AgentMovement.Example;
using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Domain.Inventory.Example;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Example;
using Game.Systems.Orchestration.Runtime.Core;
using Game.Systems.Orchestration.Runtime.Interfaces;

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
        private readonly IAgentMovementSimulation _movementSimulation;

        public AgentMovementRuntimeAdapter(IAgentMovementSimulation movementSimulation)
        {
            _movementSimulation = movementSimulation ?? throw new System.ArgumentNullException(nameof(movementSimulation));
        }

        public void Tick(float deltaTime) => _movementSimulation.AdvanceSimulation(deltaTime);
    }

    public static RuntimeSystem CreateRuntime()
    {
        // Foundation
        var math = new GameMathSystem();

        // Gameplay systems
        var movement = new AgentMovementSystem(math);
        var movementAdapter = new AgentMovementRuntimeAdapter(movement.Simulation);

        // Ordered schedule (lower Order ticks earlier)
        var entries = new List<TickEntry>
        {
            new(movementAdapter, Order: 100),
        };

        return new RuntimeSystem(new TickSchedule(entries));
    }

    public static void Main()
    {
        GameMathDemo.Run();
        Console.WriteLine();
        InventoryDemo.Run();
        Console.WriteLine();
        AgentMovementDemo.Run();

        Console.WriteLine();
        Console.WriteLine("Boot runtime tick once...");
        var runtime = CreateRuntime();
        runtime.Tick(1f / 60f);

		new AgentCommandDemo().Run();
	}
}

