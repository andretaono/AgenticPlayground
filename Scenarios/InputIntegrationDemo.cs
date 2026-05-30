using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Integration.Adapters;
using Game.Scenarios.Core.Interfaces;

namespace Game.Scenarios;

/// <summary>
/// Console demo: polls WASD via InputToCommandAdapter (ITickable),
/// forwards MoveCommand → movement.Input via CommandToMovementAdapter (ITickable),
/// advances movement simulation and prints position periodically.
/// Runs for a fixed number of ticks (approx 10s) to avoid relying on interactive Esc handling.
/// </summary>
public class InputIntegrationDemo : IScenario
{
	public string Name => "input-integration";

	public void Run()
    {
        Console.WriteLine("Input Integration Demo - press W/A/S/D to move. Running for ~10 seconds...");

        // Foundation + systems
        var math = new GameMathSystem();
        var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
        var commandSystem = new AgentCommandSystem();

        // Register agent and create entity state (AgentId -> EntityId by value)
        var agentId = new AgentId(1);
        var entity = new EntityId(agentId.Value);
        commandSystem.RegisterAgent(agentId);
        movement.Registry.CreateAgent(entity, math.Create(0f, 0f, 0f));

        // Adapters
        var inputAdapter = new InputToCommandAdapter(commandSystem, agentId);
        var commandExecution = new AgentCommandExecutionAdapter(commandSystem, movement.Input, math);
        var movementAdapter = new AgentMovementSimulationAdapter(movement.Simulation);

        // Build runtime schedule: input -> commands->movement -> simulate
        var entries = new List<TickEntry>
        {
            new TickEntry(inputAdapter, Order: 50),
            new TickEntry(commandExecution, Order: 75),
            new TickEntry(movementAdapter, Order: 100)
        };

        var runtime = new RuntimeSystem(new SimpleSchedule(entries));

        // Run loop: ~600 ticks @60Hz => ~10s. Print every 10 frames to reduce console spam.
        const int totalTicks = 600;
        for (int i = 0; i < totalTicks; i++)
        {
            runtime.Tick(1f / 60f);

            if (i % 10 == 0)
            {
                var pos = movement.Input.GetPosition(entity);
                var state = movement.Input.GetMovementState(entity);
                Console.WriteLine($"t={(i+1)/60.0:F2}s Pos=({pos.X:F2},{pos.Y:F2},{pos.Z:F2}) State={state}");
            }

            Thread.Sleep(16); // approximate 60Hz
        }

        Console.WriteLine("Demo finished.");
    }

    private sealed class SimpleSchedule : ITickSchedule
    {
        public IReadOnlyList<TickEntry> Entries { get; }
        public SimpleSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
    }
}