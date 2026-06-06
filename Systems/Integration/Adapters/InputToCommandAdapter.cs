using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentCommand.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Maps input from IInputSource into MoveCommand submissions to the AgentCommandSystem.
/// </summary>
public sealed class InputToCommandAdapter : ITickable
{
	private readonly IInputSource _inputSource;
	private readonly IAgentCommandSystem _commandSystem;
	private readonly AgentId _agentId;

	public InputToCommandAdapter(
		IInputSource inputSource,
		IAgentCommandSystem commandSystem,
		AgentId agentId)
	{
		_inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
		_commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
		_agentId = agentId;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		var direction = _inputSource.PollMovementInput(_agentId);
		if (direction.Magnitude() <= 1e-6f)
			return;

		_commandSystem.SubmitCommand(new MoveCommand(_agentId, direction));
	}
}
