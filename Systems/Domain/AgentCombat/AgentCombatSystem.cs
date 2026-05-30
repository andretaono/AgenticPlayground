using Game.Systems.Domain.AgentCombat.Controller;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat;

public sealed class AgentCombatSystem
{
	public ICombatEntityRegistry Registry { get; }
	public IAgentCombatSimulation Simulation { get; }

	public AgentCombatSystem(IAbilityExecutor abilityExecutor)
	{
		if (abilityExecutor is null)
			throw new ArgumentNullException(nameof(abilityExecutor));

		var registry = new CombatEntityRegistryController();
		var orchestrator = new AbilityOrchestrator(abilityExecutor, registry);

		Registry = registry;
		Simulation = new AgentCombatSimulationController(orchestrator, registry);
	}
}
