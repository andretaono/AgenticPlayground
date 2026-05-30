using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentCommand.Ports;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.AgentMovement.Ports;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

public sealed class AgentCommandExecutionAdapter : ITickable
{
	private readonly IAgentCommandSystem _commandSystem;
	private readonly IAgentMovementController _movementController;
	private readonly IGameMath _math;
	private readonly ICombatEntityRegistry? _combatRegistry;

	public AgentCommandExecutionAdapter(
		IAgentCommandSystem commandSystem,
		IAgentMovementController movementController,
		IGameMath math,
		ICombatEntityRegistry? combatRegistry = null)
	{
		_commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
		_movementController = movementController ?? throw new ArgumentNullException(nameof(movementController));
		_math = math ?? throw new ArgumentNullException(nameof(math));
		_combatRegistry = combatRegistry;
	}

	public void Tick(float deltaTime)
	{
		foreach (var command in _commandSystem.GetCommands())
		{
			switch (command)
			{
				case MoveCommand move:
					ApplyMoveCommand(move);
					break;
				case AttackCommand attack:
					ArmAttackCommand(attack);
					break;
			}
		}

		_commandSystem.ClearCommands();
	}

	private void ApplyMoveCommand(MoveCommand move)
	{
		var direction = _math.Create(move.Direction.X, move.Direction.Y, 0f);
		var entity = new EntityId(move.Agent.Value);
		_movementController.ApplyMovement(entity, direction);
	}

	private void ArmAttackCommand(AttackCommand attack)
	{
		if (_combatRegistry is null)
			return;

		if (!_combatRegistry.TryGet(new EntityId(attack.Agent.Value), out var attacker))
			return;

		attacker.PendingAttackTarget = attack.Target;
	}
}
