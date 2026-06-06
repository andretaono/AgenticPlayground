using Game.Scenarios.Runners;
using Xunit;

namespace Game.Tests.Scenarios;

public sealed class PolarBearScenarioTests
{
	[Fact]
	public void Run_BearCommitsAttackWithinTimeLimit()
	{
		var result = new PolarBearScenarioRunner().Run();

		Assert.True(result.TrackingDetected);
		Assert.True(result.AttackCommitted);
		Assert.True(result.FirstAttackTick > 0);
		Assert.True(result.FinalPlayerHealth < result.InitialPlayerHealth);
	}

	[Fact]
	public void Run_BehaviourTraceIncludesTrackBeforeAttack()
	{
		var result = new PolarBearScenarioRunner().Run();

		Assert.True(result.AttackCommitted);

		var trackIndex = IndexOf(result.BehaviourTrace, "polar-bear-track");
		var attackIndex = IndexOf(result.BehaviourTrace, "polar-bear-attack");

		Assert.True(trackIndex >= 0, "Expected polar-bear-track in behaviour trace.");
		Assert.True(attackIndex >= 0, "Expected polar-bear-attack in behaviour trace.");
		Assert.True(trackIndex < attackIndex, "Expected track before attack in behaviour trace.");
	}

	[Fact]
	public void Run_CanAttackWithoutLowHealth_WhenPresenceOrAwarenessQualifies()
	{
		var result = new PolarBearScenarioRunner().Run();

		Assert.True(result.AttackCommitted);
		Assert.True(result.AdvantageWithoutLowHealth);
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
