using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Controller;

public sealed class AgentCombatSimulationController : IAgentCombatSimulation
{
	private readonly AbilityOrchestrator _orchestrator;
	private readonly ICombatEntityRegistry _registry;

	public AgentCombatSimulationController(
		AbilityOrchestrator orchestrator,
		ICombatEntityRegistry registry)
	{
		_orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
	}

	public void Tick(float deltaTime)
	{
		if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
			throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

		_orchestrator.Update();
		ClearPendingAttackTargets();
	}

	private void ClearPendingAttackTargets()
	{
		foreach (var entity in _registry.GetAllEntities())
			entity.PendingAttackTarget = null;
	}
}
