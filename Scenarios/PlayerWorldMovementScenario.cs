using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.AgentCommand.Controller;
using Game.Systems.Domain.AgentMovement.Controller;
using Game.Systems.Domain.AgentMovement.Model;
using Game.Systems.Domain.World;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.GameMath.Core;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Adapters;
using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Scenarios;

/// <summary>
/// Interactive scenario: WASD moves a player agent on a visible world map.
/// Walls block movement; water is walkable (swimable tile).
/// Press Esc to exit.
/// </summary>
public sealed class PlayerWorldMovementScenario : IScenario
{
	public string Name => "player-world-movement";

	public void Run()
	{
		Console.CursorVisible = false;
		Console.WriteLine("Player World Movement");
		Console.WriteLine("W/A/S/D to move, Esc to quit.\n");
		Console.WriteLine("Legend: @ = player, W = wall, . = ground, ~ = water");
		Console.WriteLine("Press ENTER to start...");
		Console.ReadLine();

		var worldData = CreateWorldDataSource();
		var world = new WorldSystem(worldData);
		var tileRules = new DefaultTileRulesProvider();
		var visual = new DefaultTileVisualMapper();
		var renderer = new ConsoleWorldRenderer(visual);

		var math = new GameMathSystem();
		var movementPolicy = new AgentMovementPolicy(tileRules, worldData);
		var movementConfig = new AgentMovementConfig(
			GroundSpeed: 4f,
			SwimSpeed: 2.5f,
			AirSpeed: 4f);
		var movement = new AgentMovementSystem(math, movementPolicy, movementConfig);

		var commandSystem = new AgentCommandSystem();
		var agentId = new AgentId(1);
		var entityId = new EntityId(agentId.Value);
		commandSystem.RegisterAgent(agentId);

		const float startX = 10f;
		const float startY = 6f;
		movement.Registry.CreateAgent(entityId, math.Create(startX, startY, 0f));

		var inputAdapter = new InputToCommandAdapter(commandSystem, agentId);
		var commandToMovement = new CommandToMovementAdapter(commandSystem, movement.Input, math);
		var movementSimulation = new AgentMovementSimulationAdapter(movement.Simulation);

		var runtime = new RuntimeSystem(new SimpleSchedule(new[]
		{
			new TickEntry(commandToMovement, Order: 75),
			new TickEntry(movementSimulation, Order: 100)
		}));

		const float deltaTime = 1f / 60f;
		var quit = false;

		while (!quit)
		{
			Console.SetCursorPosition(0, 6);
			Console.WriteLine(new string(' ', Console.WindowWidth - 1));

			var position = movement.Input.GetPosition(entityId);
			var tile = WorldPosition.FromWorldUnits(position.X, position.Y, worldData.TileSize);
			var playerTileX = tile.X;
			var playerTileY = tile.Y;

			Console.SetCursorPosition(0, 6);
			renderer.Render(world, worldData.Width, worldData.Height, playerTileX, playerTileY);

			Console.WriteLine();
			Console.WriteLine($"Pos=({position.X:F2}, {position.Y:F2})  Tile=({playerTileX}, {playerTileY})  State={movement.Input.GetMovementState(entityId)}");

			if (Console.KeyAvailable)
			{
				var keys = new HashSet<ConsoleKey>();
				while (Console.KeyAvailable)
				{
					var keyInfo = Console.ReadKey(intercept: true);
					if (keyInfo.Key == ConsoleKey.Escape)
					{
						quit = true;
						break;
					}

					keys.Add(keyInfo.Key);
				}

				if (keys.Count > 0)
					inputAdapter.OnKeys(keys);
			}

			runtime.Tick(deltaTime);
			Thread.Sleep(16);
		}

		Console.CursorVisible = true;
		Console.Clear();
		Console.WriteLine("Scenario finished.");
	}

	private static InMemoryWorldDataSource CreateWorldDataSource()
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

	private sealed class SimpleSchedule : ITickSchedule
	{
		public IReadOnlyList<TickEntry> Entries { get; }

		public SimpleSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
	}
}
