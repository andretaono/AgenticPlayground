using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Integration.Combat;

public sealed class PendingTargetTrigger : IAbilityTrigger
{
	private readonly ICombatEntity _owner;

	public Ability Ability { get; }

	public PendingTargetTrigger(Ability ability, ICombatEntity owner)
	{
		Ability = ability ?? throw new ArgumentNullException(nameof(ability));
		_owner = owner ?? throw new ArgumentNullException(nameof(owner));
	}

	public bool IsTriggered() => _owner.PendingAttackTarget.HasValue;
}
