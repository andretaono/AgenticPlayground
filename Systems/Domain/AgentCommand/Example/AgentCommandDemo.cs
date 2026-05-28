using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Domain.AgentCommand.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.AgentCommand.Example;

public class AgentCommandDemo
{
    public void Run()
    {
        var system = new AgentCommandSystem();

        var a1 = new AgentId(1);
        var a2 = new AgentId(2);

        system.RegisterAgent(a1);
        system.RegisterAgent(a2);

        system.SubmitCommand(new MoveCommand(a1, new Vector2(1f, 0f)));
        system.SubmitCommand(new AttackCommand(a1));

        Console.WriteLine($"Has commands: {system.HasCommands()}");

        foreach (var c in system.GetCommands())
        {
            Console.WriteLine(c.GetType().Name);
        }

        system.ClearCommands();
        Console.WriteLine($"Has commands after clear: {system.HasCommands()}");
    }
}
