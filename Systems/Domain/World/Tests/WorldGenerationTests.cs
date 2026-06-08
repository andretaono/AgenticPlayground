using Game.Systems.Domain.World.Generation;
using Game.Systems.Domain.World.Generation.Controller;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.World.Tests;

public sealed class WorldGenerationTests : ITestSuite
{
	public string Name => "unit/world-generation";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "same seed produces identical map", SameSeedProducesIdenticalMap);
		registry.Add(Name, "generated map has ground path from start to goal", GeneratedMapHasGroundPath);
		registry.Add(Name, "start and goal tiles are ground", StartAndGoalTilesAreGround);
		registry.Add(Name, "border cells are wall", BorderCellsAreWall);
		registry.Add(Name, "small grid completes within max attempts", SmallGridCompletesWithinMaxAttempts);
		registry.Add(Name, "water placement creates carvable pools", WaterPlacementCreatesPools);
		registry.Add(Name, "water placement preserves ground path", WaterPlacementPreservesGroundPath);
		registry.Add(Name, "start and goal are never water", StartAndGoalAreNeverWater);
	}

	private static WorldGenerationSystem CreateSystem() => new();

	private static WorldGenerationConfig DefaultConfig() => new()
	{
		Width = 32,
		Height = 24,
		Seed = 42,
		FillProbability = 0.48f,
		CellularAutomataIterations = 5,
		MaxAttempts = 50
	};

	private static void SameSeedProducesIdenticalMap()
	{
		var system = CreateSystem();
		var config = DefaultConfig();

		var first = system.Generator.Generate(config);
		var second = system.Generator.Generate(config);

		TestAssert.Equal(first.SeedUsed, second.SeedUsed);
		TestAssert.Equal(first.Start, second.Start);
		TestAssert.Equal(first.Goal, second.Goal);

		for (var y = 0; y < first.Height; y++)
		for (var x = 0; x < first.Width; x++)
			TestAssert.Equal(first.Tiles[x, y].Id, second.Tiles[x, y].Id);
	}

	private static void GeneratedMapHasGroundPath()
	{
		var system = CreateSystem();
		var config = DefaultConfig();

		for (var seed = 1; seed <= 10; seed++)
		{
			var map = system.Generator.Generate(new WorldGenerationConfig
			{
				Width = config.Width,
				Height = config.Height,
				Seed = seed,
				FillProbability = config.FillProbability,
				CellularAutomataIterations = config.CellularAutomataIterations,
				MaxAttempts = config.MaxAttempts
			});
			TestAssert.True(GroundConnectivity.HasGroundPath(map.Tiles, map.Start, map.Goal));
		}
	}

	private static void StartAndGoalTilesAreGround()
	{
		var system = CreateSystem();
		var map = system.Generator.Generate(DefaultConfig());

		TestAssert.Equal(TileIds.Ground.Id, map.Tiles[map.Start.X, map.Start.Y].Id);
		TestAssert.Equal(TileIds.Ground.Id, map.Tiles[map.Goal.X, map.Goal.Y].Id);
	}

	private static void BorderCellsAreWall()
	{
		var system = CreateSystem();
		var map = system.Generator.Generate(DefaultConfig());

		for (var x = 0; x < map.Width; x++)
		{
			TestAssert.Equal(TileIds.Wall.Id, map.Tiles[x, 0].Id);
			TestAssert.Equal(TileIds.Wall.Id, map.Tiles[x, map.Height - 1].Id);
		}

		for (var y = 0; y < map.Height; y++)
		{
			TestAssert.Equal(TileIds.Wall.Id, map.Tiles[0, y].Id);
			TestAssert.Equal(TileIds.Wall.Id, map.Tiles[map.Width - 1, y].Id);
		}
	}

	private static void WaterPlacementCreatesPools()
	{
		var system = CreateSystem();
		var map = system.Generator.Generate(DefaultConfig());

		TestAssert.True(CountTiles(map.Tiles, TileIds.Water) > 0);
	}

	private static void WaterPlacementPreservesGroundPath()
	{
		var system = CreateSystem();

		for (var seed = 1; seed <= 10; seed++)
		{
			var map = system.Generator.Generate(new WorldGenerationConfig
			{
				Width = 32,
				Height = 24,
				Seed = seed,
				WaterPoolAttempts = 12,
				WaterPoolMaxSize = 5
			});

			TestAssert.True(GroundConnectivity.HasGroundPath(map.Tiles, map.Start, map.Goal));
		}
	}

	private static void StartAndGoalAreNeverWater()
	{
		var system = CreateSystem();
		var map = system.Generator.Generate(DefaultConfig());

		TestAssert.NotEqual(TileIds.Water.Id, map.Tiles[map.Start.X, map.Start.Y].Id);
		TestAssert.NotEqual(TileIds.Water.Id, map.Tiles[map.Goal.X, map.Goal.Y].Id);
	}

	private static int CountTiles(TileId[,] tiles, TileId tileId)
	{
		var count = 0;
		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);

		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
		{
			if (tiles[x, y] == tileId)
				count++;
		}

		return count;
	}

	private static void SmallGridCompletesWithinMaxAttempts()
	{
		var system = CreateSystem();
		var config = new WorldGenerationConfig
		{
			Width = 16,
			Height = 16,
			Seed = 7,
			FillProbability = 0.48f,
			CellularAutomataIterations = 5,
			MaxAttempts = 50
		};

		var map = system.Generator.Generate(config);

		TestAssert.True(map.Width == 16);
		TestAssert.True(map.Height == 16);
		TestAssert.True(map.SeedUsed >= config.Seed);
		TestAssert.True(map.SeedUsed < config.Seed + config.MaxAttempts);
	}
}
