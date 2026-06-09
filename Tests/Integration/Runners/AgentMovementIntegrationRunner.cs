using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Player;
using Game.Tests.Integration.Fixtures;

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

	public AgentMovementPerEntitySpeedResult RunPerEntitySpeed()
	{
		var math = new GameMathSystem();
		var playerConfig = new PlayerConfig
		{
			GroundSpeed = IntegrationTestConfigs.PerEntitySpeedTestPlayerGround
		};
		var bearConfig = new PolarBearConfig
		{
			GroundSpeed = IntegrationTestConfigs.PerEntitySpeedTestBearGround
		};
		var movement = new AgentMovementSystem(
			math,
			new PermissiveMovementPolicy(),
			playerConfig.ToMovementConfig());

		var player = new EntityId(1);
		var bear = new EntityId(2);
		movement.Registry.CreateAgent(player, math.Create(0f, 0f, 0f), playerConfig.ToMovementConfig());
		movement.Registry.CreateAgent(bear, math.Create(0f, 0f, 0f), bearConfig.ToMovementConfig());

		const float deltaTime = 1f;
		movement.Input.ApplyMovement(player, math.Create(1f, 0f, 0f));
		movement.Input.ApplyMovement(bear, math.Create(1f, 0f, 0f));
		movement.Simulation.AdvanceSimulation(deltaTime);

		var playerPosition = movement.Input.GetPosition(player);
		var bearPosition = movement.Input.GetPosition(bear);

		return new AgentMovementPerEntitySpeedResult(
			PlayerDistance: playerPosition.X,
			BearDistance: bearPosition.X);
	}
}

public sealed record AgentMovementIntegrationResult(
	int FramesSimulated,
	float FinalX,
	float FinalY,
	float FinalVelocityX);

public sealed record AgentMovementPerEntitySpeedResult(
	float PlayerDistance,
	float BearDistance);
