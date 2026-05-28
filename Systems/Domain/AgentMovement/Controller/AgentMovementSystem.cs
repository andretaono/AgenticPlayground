using Game.Systems.Domain.AgentMovement.Interfaces;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Domain.AgentMovement.Controller;

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
        var checkedMath = math ?? throw new ArgumentNullException(nameof(math));
        var checkedConfig = config ?? AgentMovementConfig.Default;
        var store = new AgentMovementStateStore();

        Registry = new AgentMovementRegistry(checkedMath, store);
        Input = new AgentMovementController(checkedMath, store);
        Simulation = new AgentMovementSimulation(checkedMath, checkedConfig, store);
    }
}

