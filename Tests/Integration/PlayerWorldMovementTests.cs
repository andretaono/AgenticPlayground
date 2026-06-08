using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class PlayerWorldMovementTests : ITestSuite
{
	public string Name => "player-world-movement";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "moves player on open ground", () =>
		{
			var result = new PlayerWorldMovementIntegrationRunner().Run();
			TestAssert.True(result.MovedEast);
			TestAssert.True(result.IsInBounds);
		});

		registry.Add(Name, "blocks movement at internal wall", () =>
		{
			var result = new PlayerWorldMovementIntegrationRunner().Run();
			TestAssert.True(result.BlockedByInternalWall);
		});

		registry.Add(Name, "allows movement onto water tiles", () =>
		{
			var result = new PlayerWorldMovementIntegrationRunner().Run();
			TestAssert.True(result.WaterTileWalkable);
		});
	}
}
