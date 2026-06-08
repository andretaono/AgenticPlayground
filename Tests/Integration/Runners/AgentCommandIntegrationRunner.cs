using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentCommand.Model;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Tests.Integration.Runners;

public sealed class AgentCommandIntegrationRunner
{
	public AgentCommandIntegrationResult Run()
	{
		var system = new AgentCommandSystem();
		var agentOne = new AgentId(1);
		var agentTwo = new AgentId(2);

		system.RegisterAgent(agentOne);
		system.RegisterAgent(agentTwo);

		system.SubmitCommand(new MoveCommand(agentOne, new Vector2(1f, 0f)));
		system.SubmitCommand(new AttackCommand(agentOne, new EntityId(99)));

		var hasCommandsBeforeClear = system.HasCommands();
		var commandTypes = system.GetCommands()
			.Select(command => command.GetType().Name)
			.ToList();

		system.ClearCommands();
		var hasCommandsAfterClear = system.HasCommands();

		return new AgentCommandIntegrationResult(
			HasCommandsBeforeClear: hasCommandsBeforeClear,
			CommandTypes: commandTypes,
			HasCommandsAfterClear: hasCommandsAfterClear);
	}
}

public sealed record AgentCommandIntegrationResult(
	bool HasCommandsBeforeClear,
	IReadOnlyList<string> CommandTypes,
	bool HasCommandsAfterClear);
