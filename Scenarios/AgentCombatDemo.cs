using Game.Scenarios.Core.Interfaces;
using Game.Scenarios.Runners;

namespace Game.Scenarios;

public sealed class AgentCombatDemo : IScenario
{
	public string Name => "agent-combat";

	public void Run()
	{
		Console.WriteLine("=== Agent Combat: chase, attack, apply damage ===");

		var result = new AgentCombatScenarioRunner().Run();

		Console.WriteLine($"Initial distance: {result.InitialDistance:F1}");
		Console.WriteLine($"Final distance: {result.FinalDistance:F1}");
		Console.WriteLine(
			$"Target health: {result.FinalTargetHealth:F1}/{result.InitialTargetHealth:F1}");
		Console.WriteLine($"Target damaged: {result.TargetDamaged}");
	}
}
