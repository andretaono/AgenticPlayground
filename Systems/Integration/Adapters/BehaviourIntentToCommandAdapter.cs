using Game.Systems.Domain.AgentBehaviour.Model;
using Game.Systems.Domain.AgentBehaviour.Ports;
using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Domain.AgentCommand.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Converts behaviour intents emitted by AgentBehaviour into AgentCommands.
/// </summary>
public sealed class BehaviourIntentToCommandAdapter
{
	private readonly IAgentBehaviourOutput _behaviourOutput;
	private readonly IAgentCommandSystem _commandSystem;

	public BehaviourIntentToCommandAdapter(
		IAgentBehaviourOutput behaviourOutput,
		IAgentCommandSystem commandSystem)
	{
		_behaviourOutput = behaviourOutput ?? throw new ArgumentNullException(nameof(behaviourOutput));
		_commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));
	}

	public void SubmitEmittedIntents(AgentId agentId)
	{
		foreach (var intent in _behaviourOutput.GetEmittedIntents(agentId))
		{
			switch (intent)
			{
				case MoveBehaviourIntent move:
					_commandSystem.SubmitCommand(new MoveCommand(move.Agent, move.Direction));
					break;
				case AttackBehaviourIntent attack:
					_commandSystem.SubmitCommand(new AttackCommand(attack.Agent));
					break;
			}
		}
	}
}
