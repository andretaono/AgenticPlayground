using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class CooldownRecordingAbilityExecutor : IAbilityExecutor
{
	private readonly IAbilityExecutor _inner;
	private readonly CombatRuntimeServices _services;

	public CooldownRecordingAbilityExecutor(IAbilityExecutor inner, CombatRuntimeServices services)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_services = services ?? throw new ArgumentNullException(nameof(services));
	}

	public AbilityExecutionResult Execute(Ability ability, AbilityContext context)
	{
		var result = _inner.Execute(ability, context);
		if (!context.Source.PendingAttackTarget.HasValue)
			return result;

		_services.CooldownTracker.MarkUsed(context.Source.EntityId, _services.CurrentTime);
		if (_services.TryGetAbilityDefinition(ability, out var definition))
		{
			_services.FeedbackStore.RecordSwing(
				context.Source.EntityId,
				_services.Orientation.GetForward(context.Source.EntityId),
				definition.Range,
				definition.ArcDegrees,
				_services.CurrentTime);
		}

		return result;
	}
}
