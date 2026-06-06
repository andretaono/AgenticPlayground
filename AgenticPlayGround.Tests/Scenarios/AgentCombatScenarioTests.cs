using Game.Scenarios.Runners;
using Xunit;

namespace Game.Tests.Scenarios;

public sealed class AgentCombatScenarioTests
{
	[Fact]
	public void Run_AttackerReachesTargetAndAppliesDamage()
	{
		var result = new AgentCombatScenarioRunner().Run();

		Assert.True(result.TargetDamaged);
		Assert.True(result.FinalDistance <= 2.5f);
		Assert.True(result.FinalTargetHealth < result.InitialTargetHealth);
	}
}
