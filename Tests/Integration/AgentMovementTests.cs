using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class AgentMovementTests : ITestSuite
{
	public string Name => "agent-movement";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "applies horizontal input over multiple frames", () =>
		{
			var result = new AgentMovementIntegrationRunner().Run();
			TestAssert.Equal(3, result.FramesSimulated);
			TestAssert.True(result.FinalX > 0f);
			TestAssert.Equal(0f, result.FinalY);
		});
	}
}
