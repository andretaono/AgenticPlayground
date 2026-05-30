using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Model;

internal sealed class AbilityExecutionState
{
	public float TotalValueApplied { get; private set; }
	public IReadOnlyList<ICombatEntity> AffectedTargets => _affectedTargets;

	private readonly List<ICombatEntity> _affectedTargets = new();

	public void RegisterApplication(ICombatEntity target, float value)
	{
		if (!_affectedTargets.Contains(target))
			_affectedTargets.Add(target);

		TotalValueApplied += value;
	}

	public AbilityExecutionResult ToResult() =>
		new(_affectedTargets.AsReadOnly(), TotalValueApplied);
}
