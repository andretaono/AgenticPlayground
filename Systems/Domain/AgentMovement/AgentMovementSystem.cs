using Game.Systems.Domain.AgentMovement.Controller;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Domain.AgentMovement;

/// <summary>
/// Root orchestrator: wires registry, input, and simulation controllers.
/// </summary>
public sealed class AgentMovementSystem : IAgentMovementSystem
{
	public IAgentMovementRegistry Registry { get; }
	public IAgentMovementController Input { get; }
	public IAgentMovementSimulation Simulation { get; }

	public AgentMovementSystem(
		IGameMath math,
		IAgentMovementPolicy movementPolicy,
		AgentMovementConfig? config = null)
	{
		var checkedMath = math ?? throw new ArgumentNullException(nameof(math));
		var checkedConfig = config ?? new AgentMovementConfig(5f, 3f, 4f);
		var store = new AgentMovementStateStore();

		Registry = new AgentMovementRegistryController(checkedMath, store, checkedConfig);
		Input = new AgentMovementController(checkedMath, store);
		Simulation = new AgentMovementSimulation(checkedMath, store, movementPolicy);
	}
}
