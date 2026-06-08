using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class AgentCommandTests : ITestSuite
{
	public string Name => "agent-command";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "buffers move and attack commands", () =>
		{
			var result = new AgentCommandIntegrationRunner().Run();
			TestAssert.True(result.HasCommandsBeforeClear);
			TestAssert.True(result.CommandTypes.Contains("MoveCommand"));
			TestAssert.True(result.CommandTypes.Contains("AttackCommand"));
		});

		registry.Add(Name, "clears command buffer", () =>
		{
			var result = new AgentCommandIntegrationRunner().Run();
			TestAssert.False(result.HasCommandsAfterClear);
		});
	}
}
