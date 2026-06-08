using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.World;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Integration.Runtime;

namespace Game.Tests.Integration.Runners;

public sealed class PlayerWorldMovementIntegrationRunner
{
	private const float StartX = 10f;
	private const float StartY = 6f;
	private const float DeltaTime = 1f / 60f;

	public PlayerWorldMovementIntegrationResult Run()
	{
		var worldData = CreateWorldDataSource();
		var world = new WorldSystem(worldData);
		var tileRules = new DefaultTileRulesProvider();
		var movementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementConfig = new AgentMovementConfig(
			GroundSpeed: 4f,
			SwimSpeed: 2.5f,
			AirSpeed: 4f);

		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, movementPolicy, movementConfig);
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(math.Create(StartX, StartY, 0f));

		var inputSource = new ScriptedInputSource(player.AgentId, new Vector2(1f, 0f));
		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, player.AgentId)
			.Build();

		var positionAfterGround = SimulateTicks(runtime, movement, player.EntityId, ticks: 30);
		var blockedByWall = positionAfterGround.X < 14f;
		var movedEast = positionAfterGround.X > StartX;

		var canEnterWater = movementPolicy.CanMoveTo(math.Create(9f, 4.5f, 0f));

		return new PlayerWorldMovementIntegrationResult(
			StartX: StartX,
			PositionAfterGroundX: positionAfterGround.X,
			MovedEast: movedEast,
			BlockedByInternalWall: blockedByWall,
			WaterTileWalkable: canEnterWater,
			IsInBounds: world.IsInBounds(new WorldPosition(
				(int)MathF.Floor(positionAfterGround.X),
				(int)MathF.Floor(positionAfterGround.Y))));
	}

	private static IVector3 SimulateTicks(
		GameRuntime runtime,
		AgentMovementSystem movement,
		Game.Systems.Foundation.Primitives.EntityId entityId,
		int ticks)
	{
		for (var i = 0; i < ticks; i++)
			runtime.Tick(DeltaTime);

		return movement.Input.GetPosition(entityId);
	}

	internal static InMemoryWorldDataSource CreateWorldDataSource()
	{
		const int width = 20;
		const int height = 12;
		var map = new TileId[width, height];

		for (var x = 0; x < width; x++)
		for (var y = 0; y < height; y++)
			map[x, y] = new TileId("ground");

		for (var x = 0; x < width; x++)
		{
			map[x, 0] = new TileId("wall");
			map[x, height - 1] = new TileId("wall");
		}

		for (var y = 0; y < height; y++)
		{
			map[0, y] = new TileId("wall");
			map[width - 1, y] = new TileId("wall");
		}

		map[8, 4] = new TileId("water");
		map[9, 4] = new TileId("water");
		map[10, 4] = new TileId("water");
		map[11, 4] = new TileId("water");
		map[8, 5] = new TileId("water");
		map[9, 5] = new TileId("water");
		map[10, 5] = new TileId("water");
		map[11, 5] = new TileId("water");

		map[14, 3] = new TileId("wall");
		map[14, 4] = new TileId("wall");
		map[14, 5] = new TileId("wall");
		map[14, 6] = new TileId("wall");

		return new InMemoryWorldDataSource(map);
	}
}

public sealed record PlayerWorldMovementIntegrationResult(
	float StartX,
	float PositionAfterGroundX,
	bool MovedEast,
	bool BlockedByInternalWall,
	bool WaterTileWalkable,
	bool IsInBounds);
