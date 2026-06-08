using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class InputIntegrationTests : ITestSuite
{
	public string Name => "input-integration";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "input pipeline moves player east", () =>
		{
			var result = new InputIntegrationRunner().Run();
			TestAssert.True(result.InputPollCount > 0);
			TestAssert.True(result.FinalX > 0f);
			TestAssert.Equal(0f, result.FinalY);
		});
	}
}
