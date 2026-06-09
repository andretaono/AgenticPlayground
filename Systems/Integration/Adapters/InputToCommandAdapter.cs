using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentCommand.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Maps input from IInputSource into MoveCommand and AttackCommand submissions.
/// </summary>
public sealed class InputToCommandAdapter : ITickable
{
	private readonly IInputSource _inputSource;
	private readonly IAgentCommandSystem _commandSystem;
	private readonly AgentId _agentId;
	private readonly EntityId _entityId;
	private readonly GameSessionState? _sessionState;
	private readonly AttackCooldownTracker? _cooldownTracker;
	private readonly ArcAttackAbilityDefinition? _attackAbility;
	private readonly CombatRuntimeServices? _combatServices;

	public InputToCommandAdapter(
		IInputSource inputSource,
		IAgentCommandSystem commandSystem,
		AgentId agentId,
		EntityId entityId,
		GameSessionState? sessionState = null,
		AttackCooldownTracker? cooldownTracker = null,
		ArcAttackAbilityDefinition? attackAbility = null,
		CombatRuntimeServices? combatServices = null)
	{
		_inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
		_commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
		_agentId = agentId;
		_entityId = entityId;
		_sessionState = sessionState;
		_cooldownTracker = cooldownTracker;
		_attackAbility = attackAbility;
		_combatServices = combatServices;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		if (_sessionState is not null && _sessionState.PlayerIsDead)
			return;

		var direction = _inputSource.PollMovementInput(_agentId);
		if (direction.Magnitude() > 1e-6f)
			_commandSystem.SubmitCommand(new MoveCommand(_agentId, direction));

		if (!_inputSource.PollAttackInput(_agentId))
			return;

		if (_cooldownTracker is not null &&
		    _attackAbility is not null &&
		    _combatServices is not null &&
		    !_cooldownTracker.IsReady(
			    _entityId,
			    _attackAbility.CooldownSeconds,
			    _combatServices.CurrentTime))
		{
			return;
		}

		_commandSystem.SubmitCommand(new AttackCommand(_agentId, CombatAttackSentinel.ArcAttack));
	}
}
