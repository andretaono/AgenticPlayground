using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;

namespace Game.Tests.Integration.Runners;

public sealed class AgentMovementIntegrationRunner
{
	public AgentMovementIntegrationResult Run()
	{
		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, new PermissiveMovementPolicy());
		var player = new EntityId(1);

		movement.Registry.CreateAgent(player, math.Create(0f, 0f, 0f));

		const int frames = 3;
		const float deltaTime = 1f / 60f;

		for (var i = 0; i < frames; i++)
		{
			movement.Input.ApplyMovement(player, math.Create(1f, 0f, 0f));
			movement.Simulation.AdvanceSimulation(deltaTime);
		}

		var position = movement.Input.GetPosition(player);
		var velocity = movement.Input.GetVelocity(player);

		return new AgentMovementIntegrationResult(
			FramesSimulated: frames,
			FinalX: position.X,
			FinalY: position.Y,
			FinalVelocityX: velocity.X);
	}
}

public sealed record AgentMovementIntegrationResult(
	int FramesSimulated,
	float FinalX,
	float FinalY,
	float FinalVelocityX);
