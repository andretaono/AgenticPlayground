using Xunit;
using Game.Systems.Domain.AgentCommand.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Tests.AgentCommand.Tests;

public class AgentCommandSystemTests
{
    [Fact]
    public void SubmitCommand_UnregisteredAgent_Throws()
    {
        var sys = new AgentCommandSystem();
        var cmd = new MoveCommand(new AgentId(1), new Vector2(1f, 0f));
        Assert.Throws<InvalidOperationException>(() => sys.SubmitCommand(cmd));
    }

    [Fact]
    public void SubmitAndClear_CommandsBufferWorks()
    {
        var sys = new AgentCommandSystem();
        var id = new AgentId(5);
        sys.RegisterAgent(id);
        sys.SubmitCommand(new AttackCommand(id));
        Assert.True(sys.HasCommands());
        sys.ClearCommands();
        Assert.False(sys.HasCommands());
    }

    [Fact]
    public void GetCommands_ReturnsCommandsInOrder()
    {
        var sys = new AgentCommandSystem();
        var a = new AgentId(1);
        sys.RegisterAgent(a);
        sys.SubmitCommand(new MoveCommand(a, new Vector2(1f, 0f)));
        sys.SubmitCommand(new AttackCommand(a));
        var list = sys.GetCommands();
        Assert.Equal(2, list.Count);
        Assert.IsType<MoveCommand>(list[0]);
        Assert.IsType<AttackCommand>(list[1]);
    }
}
