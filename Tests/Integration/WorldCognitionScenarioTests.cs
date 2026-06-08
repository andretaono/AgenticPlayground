using Game.Tests.Integration.Runners;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class WorldCognitionScenarioTests : ITestSuite
{
	public string Name => "world-cognition";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "records presence disturbance and affinity", () =>
		{
			var result = new WorldCognitionIntegrationRunner().Run();
			TestAssert.True(result.PresenceAfterActivity > 0f);
			TestAssert.True(result.DisturbanceAfterActivity > 0f);
			TestAssert.True(result.BearAffinityAfterActivity > 0f);
			TestAssert.True(result.RavenAffinityAfterActivity > 0f);
		});

		registry.Add(Name, "derives awareness mood and ecological interest", () =>
		{
			var result = new WorldCognitionIntegrationRunner().Run();
			TestAssert.True(result.AwarenessAfterActivity == AwarenessState.Unnoticed);
			TestAssert.True(result.RegionalMoodAfterActivity == RegionalMood.Quiet);
			TestAssert.True(result.DominantInterest == AffinityType.Raven);
		});

		registry.Add(Name, "decays activity over time", () =>
		{
			var result = new WorldCognitionIntegrationRunner().Run();
			TestAssert.True(result.PresenceAfterDecay < result.PresenceAfterActivity);
			TestAssert.True(result.DisturbanceAfterDecay < result.DisturbanceAfterActivity);
		});

		registry.Add(Name, "leaves distant cells unaffected", () =>
		{
			var result = new WorldCognitionIntegrationRunner().Run();
			TestAssert.Equal(0f, result.DistantPresence);
			TestAssert.Equal(0f, result.DistantDisturbance);
		});
	}
}
