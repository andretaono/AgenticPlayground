using Game.Systems.Domain.AgentCommand;
using Game.Systems.Domain.AgentMovement;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Runtime;

namespace Game.Tests.Integration.Runners;

public sealed class TileOccupancyIntegrationRunner
{
	private const float DeltaTime = 1f / 20f;

	public TileOccupancyIntegrationResult Run()
	{
		var tiles = new TileId[3, 3];
		for (var y = 0; y < 3; y++)
		for (var x = 0; x < 3; x++)
			tiles[x, y] = TileIds.Ground;

		var worldData = new InMemoryWorldDataSource(tiles);
		var tileRules = new DefaultTileRulesProvider();
		var tilePolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementPolicy = new OccupancyAwareMovementPolicy(tilePolicy);
		var movement = new AgentMovementSystem(
			new GameMathSystem(),
			movementPolicy,
			new AgentMovementConfig(GroundSpeed: 4f, SwimSpeed: 2.5f, AirSpeed: 4f));
		var commandSystem = new AgentCommandSystem();
		var actorRegistry = new ActorRegistry(commandSystem, movement);
		movementPolicy.SetOccupancyQuery(new MovementTileOccupancyQuery(actorRegistry, movement));

		var blocker = actorRegistry.RegisterActor(new GameMathSystem().Create(1.5f, 1.5f, 0f));
		var mover = actorRegistry.RegisterActor(new GameMathSystem().Create(0.5f, 1.5f, 0f));

		var inputSource = new ScriptedInputSource(mover.AgentId, new Vector2(1f, 0f));
		var runtime = new GameRuntimeBuilder(new GameMathSystem())
			.WithExistingMovement(movement)
			.WithExistingCommand(commandSystem)
			.WithInput(inputSource, mover.AgentId)
			.Build();

		for (var i = 0; i < 120; i++)
			runtime.Tick(DeltaTime);

		var moverPosition = movement.Input.GetPosition(mover.EntityId);
		var blockerPosition = movement.Input.GetPosition(blocker.EntityId);
		var moverTile = new WorldPosition((int)MathF.Floor(moverPosition.X), (int)MathF.Floor(moverPosition.Y));
		var blockerTile = new WorldPosition((int)MathF.Floor(blockerPosition.X), (int)MathF.Floor(blockerPosition.Y));

		return new TileOccupancyIntegrationResult(
			SameTile: moverTile == blockerTile,
			MoverTileX: moverTile.X,
			BlockerTileX: blockerTile.X);
	}
}

public sealed record TileOccupancyIntegrationResult(
	bool SameTile,
	int MoverTileX,
	int BlockerTileX);
