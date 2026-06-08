using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class WorldDemoTests : ITestSuite
{
	public string Name => "world-demo";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "reads tile id at center position", () =>
		{
			var result = new WorldIntegrationRunner().Run();
			TestAssert.Equal("water", result.CenterTileId);
		});

		registry.Add(Name, "rejects out of bounds positions", () =>
		{
			var result = new WorldIntegrationRunner().Run();
			TestAssert.True(result.IsOutOfBounds);
		});

		registry.Add(Name, "returns neighborhood around center", () =>
		{
			var result = new WorldIntegrationRunner().Run();
			TestAssert.Equal(9, result.NeighborhoodCount);
			TestAssert.True(result.WaterTilesInNeighborhood > 0);
		});
	}
}
