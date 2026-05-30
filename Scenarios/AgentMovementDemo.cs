using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;

namespace Game.Scenarios;

public class AgentMovementDemo : IScenario
{
	public string Name => "agent-movement";

	public void Run()
    {
        var math = new GameMathSystem();
        var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());

        var player = new EntityId(1);
        movement.Registry.CreateAgent(player, math.Create(0f, 0f, 0f));

        // Simulate holding right for 3 frames at 60fps.
        for (var i = 0; i < 3; i++)
        {
            movement.Input.ApplyMovement(player, math.Create(1f, 0f, 0f));
            movement.Simulation.AdvanceSimulation(1f / 60f);
            Console.WriteLine($"Frame {i + 1}: pos={movement.Input.GetPosition(player)} vel={movement.Input.GetVelocity(player)}");
        }
    }
}

