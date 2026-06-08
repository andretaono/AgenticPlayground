using Game.Scenarios.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests;

public sealed class PolarBearTests : ITestSuite
{
	public string Name => "polar-bear";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "commits attack within time limit", () =>
		{
			var result = new PolarBearScenarioRunner().Run();
			TestAssert.True(result.TrackingDetected);
			TestAssert.True(result.AttackCommitted);
			TestAssert.True(result.FirstAttackTick > 0);
			TestAssert.True(result.FinalPlayerHealth < result.InitialPlayerHealth);
		});

		registry.Add(Name, "behaviour trace includes track before attack", () =>
		{
			var result = new PolarBearScenarioRunner().Run();
			TestAssert.True(result.AttackCommitted);

			var trackIndex = IndexOf(result.BehaviourTrace, "polar-bear-track");
			var attackIndex = IndexOf(result.BehaviourTrace, "polar-bear-attack");

			TestAssert.True(trackIndex >= 0, "Expected polar-bear-track in behaviour trace.");
			TestAssert.True(attackIndex >= 0, "Expected polar-bear-attack in behaviour trace.");
			TestAssert.True(trackIndex < attackIndex, "Expected track before attack in behaviour trace.");
		});

		registry.Add(Name, "can attack without low health when presence or awareness qualifies", () =>
		{
			var result = new PolarBearScenarioRunner().Run();
			TestAssert.True(result.AttackCommitted);
			TestAssert.True(result.AdvantageWithoutLowHealth);
		});
	}

	private static int IndexOf(IReadOnlyList<string> trace, string behaviourId)
	{
		for (var i = 0; i < trace.Count; i++)
		{
			if (trace[i] == behaviourId)
				return i;
		}

		return -1;
	}
}
