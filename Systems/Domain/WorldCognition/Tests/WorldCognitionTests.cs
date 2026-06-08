using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.WorldCognition.Tests;

public sealed class WorldCognitionTests : ITestSuite
{
	public string Name => "unit/world-cognition";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "add presence updates cell value", AddPresenceUpdatesCell);
		registry.Add(Name, "awareness maps presence to tracked band", AwarenessMapsToTracked);
		registry.Add(Name, "awareness maps presence to hunted band", AwarenessMapsToHunted);
		registry.Add(Name, "simulation decay reduces presence", SimulationDecayReducesPresence);
	}

	private static WorldCognitionSystem CreateIsolatedSystem(WorldCognitionConfig? config = null) =>
		new(config ?? new WorldCognitionConfig
		{
			GridWidth = 4,
			GridHeight = 4,
			CellSize = 1f,
			QueryRadiusCells = 0,
			AwarenessPresenceWeight = 1f,
			AwarenessDisturbanceWeight = 0f
		});

	private static void AddPresenceUpdatesCell()
	{
		var system = CreateIsolatedSystem();
		var position = new Vector2(0f, 0f);

		system.Cognition.AddPresence(position, 42f);

		TestAssert.Equal(42f, system.Cognition.GetCell(position).Presence);
	}

	private static void AwarenessMapsToTracked()
	{
		var system = CreateIsolatedSystem();
		var position = new Vector2(1f, 1f);

		system.Cognition.AddPresence(position, 70f);

		TestAssert.True(system.Cognition.GetAwareness(position) == AwarenessState.Tracked);
	}

	private static void AwarenessMapsToHunted()
	{
		var system = CreateIsolatedSystem();
		var position = new Vector2(2f, 2f);

		system.Cognition.AddPresence(position, 90f);

		TestAssert.True(system.Cognition.GetAwareness(position) == AwarenessState.Hunted);
	}

	private static void SimulationDecayReducesPresence()
	{
		var system = CreateIsolatedSystem(new WorldCognitionConfig
		{
			GridWidth = 4,
			GridHeight = 4,
			CellSize = 1f,
			QueryRadiusCells = 0,
			PresenceDecayPerSecond = 0.5f
		});

		var position = new Vector2(0f, 0f);
		system.Cognition.AddPresence(position, 100f);

		system.Simulation.AdvanceSimulation(1f);

		TestAssert.Equal(50f, system.Cognition.GetCell(position).Presence);
	}
}
