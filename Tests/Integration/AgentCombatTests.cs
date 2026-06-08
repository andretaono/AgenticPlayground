using Game.Scenarios.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class AgentCombatTests : ITestSuite
{
	public string Name => "agent-combat";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "attacker reaches target and applies damage", () =>
		{
			var result = new AgentCombatScenarioRunner().Run();
			TestAssert.True(result.TargetDamaged);
			TestAssert.True(result.FinalDistance <= 2.5f);
			TestAssert.True(result.FinalTargetHealth < result.InitialTargetHealth);
		});
	}
}
