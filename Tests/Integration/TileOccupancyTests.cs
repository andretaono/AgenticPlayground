using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class TileOccupancyTests : ITestSuite
{
	public string Name => "tile-occupancy";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "blocks movement into an occupied tile", () =>
		{
			var result = new TileOccupancyIntegrationRunner().Run();
			TestAssert.False(result.SameTile, "Entities should not share a tile.");
			TestAssert.True(result.MoverTileX < result.BlockerTileX, "Mover should stop before the occupied tile.");
		});
	}
}
