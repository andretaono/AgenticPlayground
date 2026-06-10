using Game.Systems.Integration.Enemies;
using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class VitalityMonitorTests : ITestSuite
{
	public string Name => "vitality-monitor";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "removes dead enemy from runtime registries", () =>
		{
			var result = new VitalityMonitorIntegrationRunner().RunEnemyCleanup();
			TestAssert.True(result.EnemyRemovedFromRegistry);
			TestAssert.True(result.EnemyRemovedFromMovement);
			TestAssert.True(result.EnemyRemovedFromCombat);
			TestAssert.True(result.TicksAfterEnemyDeathWithoutError);
		});

		registry.Add(Name, "marks player dead when health depleted", () =>
		{
			var playerDead = new VitalityMonitorIntegrationRunner().RunPlayerDeath();
			TestAssert.True(playerDead);
		});
	}
}
