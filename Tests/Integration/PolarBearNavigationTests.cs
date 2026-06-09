using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class PolarBearNavigationTests : ITestSuite
{
	public string Name => "polar-bear-navigation";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "routes around wall toward player", () =>
		{
			var result = new PolarBearNavigationIntegrationRunner().Run();

			TestAssert.True(result.TrackingDetected);
			TestAssert.True(result.ReachedStalkRange, "Bear should reach stalk range around the wall.");
			TestAssert.True(result.FinalDistance < result.InitialDistance, "Bear should reduce distance to player.");
			TestAssert.True(result.MinimumDistance < result.InitialDistance - 1f, "Bear should navigate past the wall segment.");
		});
	}
}
