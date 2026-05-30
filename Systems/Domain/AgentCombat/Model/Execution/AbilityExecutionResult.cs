using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Model;

public sealed class AbilityExecutionResult
{
	public IReadOnlyList<ICombatEntity> AffectedTargets { get; }
	public float TotalValueApplied { get; }

	internal AbilityExecutionResult(
		IReadOnlyList<ICombatEntity> affectedTargets,
		float totalValueApplied)
	{
		AffectedTargets = affectedTargets;
		TotalValueApplied = totalValueApplied;
	}
}
