using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Scenarios;

public sealed class WorldCognitionDemo : IScenario
{
	public string Name => "world-cognition";

	public void Run()
	{
		var config = new WorldCognitionConfig
		{
			GridWidth = 32,
			GridHeight = 32,
			CellSize = 32f,
			QueryRadiusCells = 2
		};

		var cognition = new WorldCognitionSystem(config);
		var position = new Vector2(64f, 64f);

		Console.WriteLine("=== Case 1: Record player activity ===");
		cognition.Cognition.AddPresence(position, WorldCognitionContributions.Presence.Movement);
		cognition.Cognition.AddPresence(position, WorldCognitionContributions.Presence.Campfire);
		cognition.Cognition.AddDisturbance(position, WorldCognitionContributions.Disturbance.EnemyKill);
		cognition.Cognition.AddAffinity(position, AffinityType.Bear, WorldCognitionContributions.Affinity.MeleeKillBear);
		cognition.Cognition.AddAffinity(position, AffinityType.Raven, WorldCognitionContributions.Affinity.DiscoveryRaven);

		PrintCell(cognition, position, "After activity");

		Console.WriteLine("\n=== Case 2: Derived regional outputs ===");
		Console.WriteLine($"Awareness: {cognition.Cognition.GetAwareness(position)}");
		Console.WriteLine($"Regional mood: {cognition.Cognition.GetRegionalMood(position)}");

		var interest = cognition.Cognition.GetEcologicalInterest(position);
		Console.WriteLine(
			$"Ecological interest: Bear={interest.Bear:F1}% Raven={interest.Raven:F1}% Seal={interest.Seal:F1}% " +
			$"(dominant={interest.DominantInterest})");

		Console.WriteLine("\n=== Case 3: Decay over 30 seconds ===");
		const float deltaTime = 1f;
		for (var second = 1; second <= 30; second++)
		{
			cognition.Simulation.AdvanceSimulation(deltaTime);

			if (second is 1 or 10 or 20 or 30)
				PrintCell(cognition, position, $"t={second}s");
		}

		Console.WriteLine("\n=== Case 4: Distant cell remains unaffected ===");
		var distant = new Vector2(512f, 512f);
		var distantCell = cognition.Cognition.GetCell(distant);
		Console.WriteLine(
			$"Distant cell presence={distantCell.Presence:F2}, disturbance={distantCell.Disturbance:F2}");
	}

	private static void PrintCell(WorldCognitionSystem cognition, Vector2 position, string label)
	{
		var cell = cognition.Cognition.GetCell(position);
		Console.WriteLine(
			$"{label}: presence={cell.Presence:F2}, disturbance={cell.Disturbance:F2}, " +
			$"bear={cell.BearAffinity:F2}, raven={cell.RavenAffinity:F2}, seal={cell.SealAffinity:F2}");
	}
}
