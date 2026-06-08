using Game.Systems.Domain.AgentBehaviour.Tests;
using Game.Systems.Domain.EntityResource.Tests;
using Game.Systems.Domain.ItemAssembly.Tests;
using Game.Systems.Domain.WorldCognition.Tests;
using Game.Systems.Foundation.Testing;
using Game.Tests.Integration;

namespace Game.Tests.Core;

public static class UnitTestRunner
{
	public static int Run(string[] args)
	{
		var filter = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : null;
		var registry = new TestRegistry();

		foreach (var suite in CreateSuites())
			suite.Register(registry);

		var result = registry.RunAll(filter);
		PrintReport(result, filter);

		return result.FailedCount > 0 ? 1 : 0;
	}

	private static IEnumerable<ITestSuite> CreateSuites()
	{
		yield return new ItemAssemblyTests();
		yield return new WorldCognitionTests();
		yield return new AgentBehaviourTests();
		yield return new EntityResourceTests();

		yield return new RuntimeTests();
		yield return new AgentCommandTests();
		yield return new AgentMovementTests();
		yield return new InputIntegrationTests();
		yield return new InventoryTests();
		yield return new WorldDemoTests();
		yield return new PlayerWorldMovementTests();
		yield return new EntityResourceScenarioTests();
		yield return new AgentBehaviourScenarioTests();
		yield return new ItemAssemblyScenarioTests();
		yield return new WorldCognitionScenarioTests();
		yield return new PolarBearTests();
		yield return new AgentCombatTests();
	}

	private static void PrintReport(TestRunResult result, string? filter)
	{
		var label = string.IsNullOrWhiteSpace(filter) ? "all" : filter;
		Console.WriteLine($"Running {result.Results.Count} test(s) [{label}]...\n");

		foreach (var caseResult in result.Results)
		{
			var status = caseResult.Passed ? "PASS" : "FAIL";
			Console.WriteLine($"  {status}  {caseResult.FullName} ({caseResult.ElapsedMilliseconds} ms)");

			if (!caseResult.Passed && !string.IsNullOrWhiteSpace(caseResult.Message))
				Console.WriteLine($"        {caseResult.Message}");
		}

		Console.WriteLine();
		Console.WriteLine(
			$"Results: {result.PassedCount} passed, {result.FailedCount} failed " +
			$"({result.TotalElapsedMilliseconds} ms)");
	}
}
