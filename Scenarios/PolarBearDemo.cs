using Game.Scenarios.Core.Interfaces;
using Game.Scenarios.Runners;

namespace Game.Scenarios;

public sealed class PolarBearDemo : IScenario
{
	public string Name => "polar-bear";

	public void Run()
	{
		Console.WriteLine("=== Polar Bear: scent detection, stalk, and ambush ===");
		Console.WriteLine("Phase 1: Player walks east, leaving a sprint scent trail.");
		Console.WriteLine("Phase 2: Player rests when bear closes in (builds vulnerable presence).");
		Console.WriteLine("Phase 3: Bear enters attack range with advantage and strikes.\n");

		var result = new PolarBearScenarioRunner().Run();

		Console.WriteLine($"Tracking detected: {result.TrackingDetected}");
		Console.WriteLine($"Attack committed: {result.AttackCommitted}");
		if (result.AttackCommitted)
		{
			Console.WriteLine($"First attack tick: {result.FirstAttackTick}");
			Console.WriteLine(
				$"Player health: {result.FinalPlayerHealth:F1}/{result.InitialPlayerHealth:F1}");
			if (result.AdvantageWithoutLowHealth)
			{
				Console.WriteLine(
					"Note: attack fired with lowHealth=false — advantage uses OR semantics " +
					"(highPresence or awarenessTracked).");
			}
		}
		else
		{
			Console.WriteLine("Bear did not reach attack conditions before the demo time limit.");
		}

		Console.WriteLine($"Behaviour trace: {string.Join(" → ", result.BehaviourTrace)}");
	}
}
