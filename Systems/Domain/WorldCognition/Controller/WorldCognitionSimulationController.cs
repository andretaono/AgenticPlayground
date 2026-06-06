using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;

namespace Game.Systems.Domain.WorldCognition.Controller;

internal sealed class WorldCognitionSimulationController : IWorldCognitionSimulation
{
	private readonly WorldCognitionGridStore _store;

	public WorldCognitionSimulationController(WorldCognitionGridStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void AdvanceSimulation(float deltaTime)
	{
		if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
			throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

		_store.ApplyDecay(deltaTime);
	}
}
