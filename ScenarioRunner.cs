using Game.Scenarios;
using Game.Scenarios.Core;

namespace Game
{
	public static class ScenarioRunner
	{
		public static void Main(string[] args)
		{
			var registry = new ScenarioRegistry();

			registry.Register(new RuntimeDemo());
			registry.Register(new AgentCommandDemo());
			registry.Register(new AgentMovementDemo());
			registry.Register(new InputIntegrationDemo());
			registry.Register(new InventoryDemo());
			registry.Register(new WorldDemo()); // newly added

			while (true)
			{
				Console.Clear();

				Console.WriteLine("Available scenarios:\n");

				var names = registry.Names.ToList();

				for (int i = 0; i < names.Count; i++)
				{
					Console.WriteLine($"{i}: {names[i]}");
				}

				Console.WriteLine("\nEnter number (or 'q' to quit):");
				Console.Write("> ");

				var input = Console.ReadLine();

				if (input == "q")
					break;

				if (int.TryParse(input, out int index) &&
					index >= 0 &&
					index < names.Count &&
					registry.TryGet(names[index], out var scenario))
				{
					Console.Clear();
					Console.WriteLine($"Running: {names[index]}\n");

					scenario.Run();

					Console.WriteLine("\nScenario finished. Press ENTER...");
					Console.ReadLine();
				}
			}
		}
	}
}
