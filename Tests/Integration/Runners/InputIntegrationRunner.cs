using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Runtime;

namespace Game.Tests.Integration.Runners;

public sealed class InputIntegrationRunner
{
	private const float DeltaTime = 1f / 60f;
	private const int TotalTicks = 120;

	public InputIntegrationResult Run()
	{
		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(math.Create(0f, 0f, 0f));

		var inputSource = new ScriptedInputSource(player.AgentId, new Vector2(1f, 0f));
		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, player.AgentId)
			.Build();

		for (var i = 0; i < TotalTicks; i++)
			runtime.Tick(DeltaTime);

		var position = movement.Input.GetPosition(player.EntityId);

		return new InputIntegrationResult(
			TicksSimulated: TotalTicks,
			InputPollCount: inputSource.PollCount,
			FinalX: position.X,
			FinalY: position.Y);
	}
}

public sealed record InputIntegrationResult(
	int TicksSimulated,
	int InputPollCount,
	float FinalX,
	float FinalY);
