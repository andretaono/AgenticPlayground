using Game.Tests.Integration.Runners;
using Game.Systems.Foundation.Testing;

namespace Game.Tests.Integration;

public sealed class AgentBehaviourScenarioTests : ITestSuite
{
	public string Name => "agent-behaviour";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "chases then attacks when in range", () =>
		{
			var result = new AgentBehaviourIntegrationRunner().RunChaseThenAttack();
			TestAssert.True(result.SawChase);
			TestAssert.True(result.SawAttack);
			TestAssert.True(result.FinalDistance <= 2f);
		});

		registry.Add(Name, "falls back to idle without target", () =>
		{
			var result = new AgentBehaviourIntegrationRunner().RunIdleFallback();
			TestAssert.Equal("idle", result.ActiveBehaviour);
		});

		registry.Add(Name, "converts intents into agent commands", () =>
		{
			var result = new AgentBehaviourIntegrationRunner().RunCommandPipeline();
			TestAssert.True(result.CommandCount > 0);
			TestAssert.True(result.CommandTypes.Contains("MoveCommand"));
		});
	}
}
