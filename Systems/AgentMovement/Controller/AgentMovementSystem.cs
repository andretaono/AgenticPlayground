using Game.AgentMovement.Interfaces;
using Game.AgentMovement.Model;
using Game.Foundation.GameMath.Interfaces;

namespace Game.AgentMovement.Controller;

/// <summary>
/// Thin system root: accepts dependencies and wires up the ports for Registry, Input, Simulation.
/// </summary>
public sealed class AgentMovementSystem : IAgentMovementSystem
{
    public IAgentMovementRegistry Registry { get; }
    public IAgentMovementController Input { get; }
    public IAgentMovementSimulation Simulation { get; }

	public AgentMovementSystem(IGameMath math, AgentMovementConfig? config = null)
    {
        var checkedMath = math ?? throw new System.ArgumentNullException(nameof(math));
        var checkedConfig = config ?? AgentMovementConfig.Default;
        var store = new AgentMovementStateStore();

        Registry = new AgentMovementRegistry(checkedMath, store);
        Input = new AgentMovementController(checkedMath, store);
        Simulation = new AgentMovementSimulation(checkedMath, checkedConfig, store);
    }
}

