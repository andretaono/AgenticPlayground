using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Presentation;
using Game.Systems.Integration.Runtime;

namespace Game.Scenarios;

/// <summary>
/// Console demo: polls WASD via ConsoleInputSource,
/// forwards MoveCommand → movement via GameRuntimeBuilder.
/// </summary>
public class InputIntegrationDemo : IScenario
{
	public string Name => "input-integration";

	public void Run()
	{
		Console.WriteLine("Input Integration Demo - press W/A/S/D to move. Running for ~10 seconds...");

		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(math.Create(0f, 0f, 0f));

		var inputSource = new ConsoleInputSource(player.AgentId);
		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, player.AgentId)
			.Build();

		const int totalTicks = 600;
		for (var i = 0; i < totalTicks; i++)
		{
			runtime.Tick(1f / 60f);

			if (i % 10 == 0)
			{
				var pos = movement.Input.GetPosition(player.EntityId);
				var state = movement.Input.GetMovementState(player.EntityId);
				Console.WriteLine($"t={(i + 1) / 60.0:F2}s Pos=({pos.X:F2},{pos.Y:F2},{pos.Z:F2}) State={state}");
			}

			Thread.Sleep(16);
		}

		Console.WriteLine("Demo finished.");
	}
}
