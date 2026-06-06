using Game.Systems.Domain.WorldCognition.Controller;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;

namespace Game.Systems.Domain.WorldCognition;

/// <summary>
/// Root orchestrator: wires cognition mutation/query and simulation controllers.
/// </summary>
public sealed class WorldCognitionSystem : IWorldCognitionSystem
{
	public IWorldCognitionController Cognition { get; }
	public IWorldCognitionSimulation Simulation { get; }

	public WorldCognitionSystem(WorldCognitionConfig? config = null)
	{
		var store = new WorldCognitionGridStore(config ?? new WorldCognitionConfig());

		Cognition = new WorldCognitionController(store);
		Simulation = new WorldCognitionSimulationController(store);
	}
}
