using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class CooldownAbilityTrigger : IAbilityTrigger
{
	private readonly ArcAttackAbilityDefinition _definition;
	private readonly ICombatEntity _owner;
	private readonly AttackCooldownTracker _cooldownTracker;
	private readonly Func<float> _currentTime;

	public CooldownAbilityTrigger(
		ArcAttackAbilityDefinition definition,
		Ability ability,
		ICombatEntity owner,
		AttackCooldownTracker cooldownTracker,
		Func<float> currentTime)
	{
		_definition = definition ?? throw new ArgumentNullException(nameof(definition));
		Ability = ability ?? throw new ArgumentNullException(nameof(ability));
		_owner = owner ?? throw new ArgumentNullException(nameof(owner));
		_cooldownTracker = cooldownTracker ?? throw new ArgumentNullException(nameof(cooldownTracker));
		_currentTime = currentTime ?? throw new ArgumentNullException(nameof(currentTime));
	}

	public Ability Ability { get; }

	public bool IsTriggered()
	{
		if (!_owner.PendingAttackTarget.HasValue)
			return false;

		return _cooldownTracker.IsReady(
			_owner.EntityId,
			_definition.CooldownSeconds,
			_currentTime());
	}
}
