using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

public sealed class WorldCognitionSimulationAdapter : ITickable
{
	private readonly IWorldCognitionSimulation _simulation;

	public WorldCognitionSimulationAdapter(IWorldCognitionSimulation simulation)
	{
		_simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
	}

	public void Tick(float deltaTime) => _simulation.AdvanceSimulation(deltaTime);
}
