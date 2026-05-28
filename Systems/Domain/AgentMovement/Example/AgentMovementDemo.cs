using Game.Systems.Domain.AgentMovement.Controller;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentMovement.Example;

public static class AgentMovementDemo
{
    public static void Run()
    {
        var math = new GameMathSystem();
        var movement = new AgentMovementSystem(math);

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

