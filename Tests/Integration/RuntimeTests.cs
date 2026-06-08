using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class RuntimeTests : ITestSuite
{
	public string Name => "runtime";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "ticks registered tickables in order", () =>
		{
			var result = new RuntimeIntegrationRunner().Run();
			TestAssert.Equal(3, result.TickCount);
			TestAssert.True(result.DeltaTime > 0f);
		});
	}
}
