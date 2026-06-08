using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class EntityResourceScenarioTests : ITestSuite
{
	public string Name => "entity-resource";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "stamina depletes to zero", () =>
		{
			var result = new EntityResourceIntegrationRunner().RunStaminaDepletion();
			TestAssert.True(result.IsDepleted);
			TestAssert.Equal(0f, result.FinalAmount);
		});

		registry.Add(Name, "mana regenerates to full", () =>
		{
			var result = new EntityResourceIntegrationRunner().RunManaRegeneration();
			TestAssert.True(result.IsFull);
			TestAssert.Equal(40f, result.FinalAmount);
		});

		registry.Add(Name, "hunger drains then health regenerates after damage", () =>
		{
			var result = new EntityResourceIntegrationRunner().RunHungerAndHealth();
			TestAssert.True(result.HungerDepleted);
			TestAssert.Equal(40f, result.HealthAfterDamage);
			TestAssert.True(result.HealthRegenerated);
			TestAssert.True(result.FinalHealth > result.HealthAfterDamage);
		});
	}
}
