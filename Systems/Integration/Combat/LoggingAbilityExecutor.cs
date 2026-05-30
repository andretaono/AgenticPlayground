using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class LoggingAbilityExecutor : IAbilityExecutor
{
	private readonly IAbilityExecutor _inner;

	public LoggingAbilityExecutor(IAbilityExecutor inner)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
	}

	public AbilityExecutionResult Execute(Ability ability, AbilityContext context)
	{
		var result = _inner.Execute(ability, context);

		if (result.AffectedTargets.Count == 0)
			return result;

		var targetIds = string.Join(", ", result.AffectedTargets.Select(t => t.EntityId.Value));
		Console.WriteLine(
			$"  Combat: source={context.Source.EntityId.Value} hit [{targetIds}] for {result.TotalValueApplied:F1} total");

		return result;
	}
}
