using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Foundation.GameMath.Interfaces;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime;

namespace Game.Tests.Integration.Runners;

public sealed class PlayerWorldMovementIntegrationRunner
{
	private const float StartX = 10f;
	private const float StartY = 6f;
	private const float DeltaTime = 1f / 60f;

	public PlayerWorldMovementSwimSpeedResult RunSwimSpeedComparison()
	{
		const int ticks = 60;
		const float deltaTime = 1f / 60f;
		const float groundStartX = 10f;
		const float groundStartY = 6f;
		const float waterStartX = 9.5f;
		const float waterStartY = 4.5f;

		var worldData = CreateWorldDataSource();
		var tileRules = new DefaultTileRulesProvider();
		var movementConfig = new AgentMovementConfig(
			GroundSpeed: 4f,
			SwimSpeed: 2.5f,
			AirSpeed: 4f);

		var groundDisplacement = SimulateDisplacement(
			worldData,
			tileRules,
			movementConfig,
			groundStartX,
			groundStartY,
			ticks,
			deltaTime);

		var waterDisplacement = SimulateDisplacement(
			worldData,
			tileRules,
			movementConfig,
			waterStartX,
			waterStartY,
			ticks,
			deltaTime);

		return new PlayerWorldMovementSwimSpeedResult(
			GroundDisplacement: groundDisplacement,
			WaterDisplacement: waterDisplacement,
			WaterSlowerThanGround: waterDisplacement < groundDisplacement * 0.85f);
	}

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

		var canEnterWater = movementPolicy.CanMoveTo(
			player.EntityId,
			math.Create(9f, 4.5f, 0f),
			bodyRadius: 0.4f);

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

	private static float SimulateDisplacement(
		InMemoryWorldDataSource worldData,
		DefaultTileRulesProvider tileRules,
		AgentMovementConfig movementConfig,
		float startX,
		float startY,
		int ticks,
		float deltaTime)
	{
		var math = new GameMathSystem();
		var movementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movement = new AgentMovementSystem(math, movementPolicy, movementConfig);
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(math.Create(startX, startY, 0f));

		var inputSource = new ScriptedInputSource(player.AgentId, new Vector2(1f, 0f));
		var stateAdapter = new AgentMovementStateAdapter(
			actorRegistry,
			movement,
			tileRules,
			worldData);

		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, player.AgentId)
			.WithExtraTickable(stateAdapter, StandardTickOrder.MovementState)
			.Build();

		for (var i = 0; i < ticks; i++)
			runtime.Tick(deltaTime);

		var position = movement.Input.GetPosition(player.EntityId);
		return position.X - startX;
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

	public bool RunWallApproachStopsSymmetrically()
	{
		const float bodyRadius = 0.4f;
		const int wallX = 10;
		const float rowY = 5.5f;
		var worldData = CreateSingleWallColumnWorldData(wallX, wallRow: 5);
		var tileRules = new DefaultTileRulesProvider();
		var movementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementConfig = new AgentMovementConfig(
			GroundSpeed: 4f,
			SwimSpeed: 2.5f,
			AirSpeed: 4f,
			BodyRadius: bodyRadius);

		var westFinal = SimulateUntilBlocked(
			worldData,
			tileRules,
			movementPolicy,
			movementConfig,
			startX: wallX - 0.5f,
			startY: rowY,
			direction: new Vector2(1f, 0f));

		var eastFinal = SimulateUntilBlocked(
			worldData,
			tileRules,
			movementPolicy,
			movementConfig,
			startX: wallX + 1.5f,
			startY: rowY,
			direction: new Vector2(-1f, 0f));

		var westGap = wallX - westFinal.X;
		var eastGap = eastFinal.X - (wallX + 1);
		return MathF.Abs(westGap - eastGap) < 0.05f
			&& westGap + 0.05f >= bodyRadius
			&& eastGap + 0.05f >= bodyRadius;
	}

	public bool RunWalkableHeightDoesNotRampIntoWall()
	{
		const int wallX = 10;
		var worldData = CreateSingleWallColumnWorldData(wallX, wallRow: 5);
		var tileRules = new DefaultTileRulesProvider();
		var samples = new float[worldData.Width, worldData.Height];
		for (var y = 0; y < worldData.Height; y++)
		for (var x = 0; x < worldData.Width; x++)
			samples[x, y] = 0f;

		samples[wallX, 5] = 1.5f;
		var heightmap = Heightmap.FromSamples(samples);
		var sampler = new WalkableSurfaceHeightSampler(tileRules, worldData);

		var nearWall = sampler.Sample(heightmap, wallX - 0.05f, 5.5f, heightScale: 1f);
		var farther = sampler.Sample(heightmap, wallX - 0.5f, 5.5f, heightScale: 1f);

		return nearWall < 0.25f && MathF.Abs(nearWall - farther) < 0.05f;
	}

	public WallSlideResult RunWallSlidePreservesParallelMotionDetails()
	{
		const int wallX = 10;
		const float bodyRadius = 0.4f;
		var worldData = CreateVerticalWallStripWorldData(wallX, yMin: 1, yMax: 10);
		var tileRules = new DefaultTileRulesProvider();
		var movementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementConfig = new AgentMovementConfig(
			GroundSpeed: 4f,
			SwimSpeed: 2.5f,
			AirSpeed: 4f,
			BodyRadius: bodyRadius);

		var startX = wallX - 1f;
		const float startY = 5.5f;
		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, movementPolicy, movementConfig);
		var player = new EntityId(42);
		movement.Registry.CreateAgent(player, math.Create(startX, startY, 0f));

		for (var i = 0; i < 45; i++)
		{
			movement.Input.ApplyMovement(player, math.Create(1f, 1f, 0f));
			movement.Simulation.AdvanceSimulation(DeltaTime);
		}

		var final = movement.Input.GetPosition(player);
		var movedAlongWall = final.Y > startY + 0.2f;
		var keptClearOfWall = final.X <= wallX - bodyRadius + 0.05f;

		return new WallSlideResult(
			final.X,
			final.Y,
			movedAlongWall,
			keptClearOfWall,
			movedAlongWall && keptClearOfWall);
	}

	public bool RunWallSlidePreservesParallelMotion() =>
		RunWallSlidePreservesParallelMotionDetails().Passed;

	private static IVector3 SimulateUntilBlocked(
		InMemoryWorldDataSource worldData,
		DefaultTileRulesProvider tileRules,
		AgentMovementPolicy movementPolicy,
		AgentMovementConfig movementConfig,
		float startX,
		float startY,
		Vector2 direction,
		int maxTicks = 600)
	{
		var math = new GameMathSystem();
		var movement = new AgentMovementSystem(math, movementPolicy, movementConfig);
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		var player = actorRegistry.RegisterActor(math.Create(startX, startY, 0f));

		var inputSource = new ScriptedInputSource(player.AgentId, direction);
		var stateAdapter = new AgentMovementStateAdapter(
			actorRegistry,
			movement,
			tileRules,
			worldData);

		var runtime = new GameRuntimeBuilder(math)
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, player.AgentId)
			.WithExtraTickable(stateAdapter, StandardTickOrder.MovementState)
			.Build();

		for (var i = 0; i < maxTicks; i++)
			runtime.Tick(DeltaTime);

		return movement.Input.GetPosition(player.EntityId);
	}

	internal static InMemoryWorldDataSource CreateVerticalWallStripWorldData(int wallX, int yMin, int yMax)
	{
		var worldData = CreateSingleWallColumnWorldData(wallX, wallRow: yMin);
		var map = worldData.LoadMap();
		for (var y = yMin + 1; y <= yMax; y++)
			map[wallX, y] = new TileId("wall");

		return new InMemoryWorldDataSource(map);
	}

	internal static InMemoryWorldDataSource CreateSingleWallColumnWorldData(int wallX, int wallRow)
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

		map[wallX, wallRow] = new TileId("wall");
		return new InMemoryWorldDataSource(map);
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

public sealed record PlayerWorldMovementSwimSpeedResult(
	float GroundDisplacement,
	float WaterDisplacement,
	bool WaterSlowerThanGround);

public sealed record PlayerWorldMovementIntegrationResult(
	float StartX,
	float PositionAfterGroundX,
	bool MovedEast,
	bool BlockedByInternalWall,
	bool WaterTileWalkable,
	bool IsInBounds);

public sealed record WallSlideResult(
	float X,
	float Y,
	bool MovedAlongWall,
	bool KeptClearOfWall,
	bool Passed);
