using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class InventoryTests : ITestSuite
{
	public string Name => "inventory";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "adds items up to capacity", () =>
		{
			var result = new InventoryIntegrationRunner().Run();
			TestAssert.True(result.AddedPotion);
			TestAssert.True(result.AddedSword);
			TestAssert.True(result.AddedQuest);
			TestAssert.Equal(3, result.CountAfterAdds);
		});

		registry.Add(Name, "removes item by id", () =>
		{
			var result = new InventoryIntegrationRunner().Run();
			TestAssert.True(result.RemovedSword);
			TestAssert.Equal(2, result.CountAfterRemoval);
			TestAssert.True(result.SwordGone);
			TestAssert.True(result.StillHasPotion);
			TestAssert.True(result.StillHasQuest);
		});
	}
}
