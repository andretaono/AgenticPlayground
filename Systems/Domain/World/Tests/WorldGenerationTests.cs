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
		registry.Add(Name, "ceiling layer dimensions match ground", CeilingLayerDimensionsMatchGround);
		registry.Add(Name, "start area has open cover", StartAreaHasOpenCover);
		registry.Add(Name, "extra wall stacks only sit on walls", ExtraWallStacksOnlyOnWalls);
		registry.Add(Name, "extra wall stacks prefer clustering", ExtraWallStacksPreferClustering);
		registry.Add(Name, "ceiling layer is seeded deterministic", CeilingLayerIsSeededDeterministic);
		registry.Add(Name, "enclosed wall blob carves interior", EnclosedWallBlobCarvesInterior);
		registry.Add(Name, "carved cave connects to exterior", CarvedCaveConnectsToExterior);
		registry.Add(Name, "too small wall blob rejected", TooSmallWallBlobRejected);
		registry.Add(Name, "max cave count limits carved caves", MaxCaveCountLimitsCarvedCaves);
		registry.Add(Name, "carved cave floor gets ceiling", CarvedCaveFloorGetsCeiling);
		registry.Add(Name, "cave carving preserves ground path", CaveCarvingPreservesGroundPath);
		registry.Add(Name, "disconnected interior pocket not carved", DisconnectedInteriorPocketNotCarved);
		registry.Add(Name, "border touching blob carves without removing border", BorderTouchingBlobCarvesWithoutRemovingBorder);
		registry.Add(Name, "max cave area caps hollow size", MaxCaveAreaCapsHollowSize);
		registry.Add(Name, "all carved cave floor reachable from start", AllCarvedCaveFloorReachableFromStart);
		registry.Add(Name, "thick wall tunnel connects cave to exterior", ThickWallTunnelConnectsCaveToExterior);
		registry.Add(Name, "border blob uses tunnel without removing border", BorderBlobUsesTunnelWithoutRemovingBorder);
		registry.Add(Name, "entrance depth limit rejects unreachable cave", EntranceDepthLimitRejectsUnreachableCave);
		registry.Add(Name, "tunnel throat cells are not cave floor", TunnelThroatCellsAreNotCaveFloor);
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

	private static void CeilingLayerDimensionsMatchGround()
	{
		var map = CreateSystem().Generator.Generate(DefaultConfig());

		TestAssert.Equal(map.Width, map.CeilingLayer.GetLength(0));
		TestAssert.Equal(map.Height, map.CeilingLayer.GetLength(1));
	}

	private static void StartAreaHasOpenCover()
	{
		var map = CreateSystem().Generator.Generate(DefaultConfig());

		TestAssert.True(map.CoverAt(map.Start.X, map.Start.Y) == CoverKind.OpenSky);
	}

	private static void ExtraWallStacksOnlyOnWalls()
	{
		var map = CreateSystem().Generator.Generate(new WorldGenerationConfig
		{
			Width = 32,
			Height = 24,
			Seed = 99,
			EnableCeilingLayer = true,
			MaxCaveCount = 0,
			ExtraWallStackChance = 0.35f,
			ExtraWallStackClusterChance = 0.9f,
			ExtraWallStackGrowPasses = 6
		});

		for (var y = 0; y < map.Height; y++)
		for (var x = 0; x < map.Width; x++)
		{
			if (map.CeilingLayer[x, y] != CeilingLayerTileIds.Solid)
				continue;

			TestAssert.Equal(TileIds.Wall.Id, map.GroundLayer[x, y].Id);
		}
	}

	private static void ExtraWallStacksPreferClustering()
	{
		var groundLayer = CreateSyntheticWallRun();
		var config = new WorldGenerationConfig
		{
			EnableCeilingLayer = true,
			ExtraWallStackChance = 0.08f,
			ExtraWallStackClusterChance = 0.85f,
			ExtraWallStackGrowPasses = 6,
			MaxCaveCount = 0
		};

		var caveRegionIndex = CreateEmptyRegionIndex(groundLayer.GetLength(0), groundLayer.GetLength(1));
		var ceilingPlacement = CeilingLayerPlacer.Place(
			groundLayer,
			new WorldPosition(1, 1),
			seed: 4242,
			config,
			caveRegionIndex);
		var ceilingLayer = ceilingPlacement.CeilingLayer;

		var stackedWalls = new List<WorldPosition>();
		for (var y = 0; y < groundLayer.GetLength(1); y++)
		for (var x = 0; x < groundLayer.GetLength(0); x++)
		{
			if (groundLayer[x, y] != TileIds.Wall)
				continue;

			if (ceilingLayer[x, y] != CeilingLayerTileIds.Solid)
				continue;

			stackedWalls.Add(new WorldPosition(x, y));
		}

		TestAssert.True(stackedWalls.Count >= 3);

		var withStackedNeighbor = 0;
		foreach (var position in stackedWalls)
		{
			if (HasStackedWallNeighbor(ceilingLayer, groundLayer, position))
				withStackedNeighbor++;
		}

		var clusterRatio = withStackedNeighbor / (float)stackedWalls.Count;
		TestAssert.True(clusterRatio >= 0.5f);
	}

	private static void CeilingLayerIsSeededDeterministic()
	{
		var config = new WorldGenerationConfig
		{
			Width = 32,
			Height = 24,
			Seed = 17,
			EnableCeilingLayer = true
		};

		var system = CreateSystem();
		var first = system.Generator.Generate(config);
		var second = system.Generator.Generate(config);

		for (var y = 0; y < first.Height; y++)
		for (var x = 0; x < first.Width; x++)
			TestAssert.Equal(first.CeilingLayer[x, y].Id, second.CeilingLayer[x, y].Id);
	}

	private static TileId[,] CreateSyntheticWallRun()
	{
		const int width = 24;
		const int height = 12;
		var tiles = new TileId[width, height];

		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
			tiles[x, y] = TileIds.Ground;

		for (var x = 4; x < 20; x++)
			tiles[x, 6] = TileIds.Wall;

		return tiles;
	}

	private static bool HasStackedWallNeighbor(
		TileId[,] ceilingLayer,
		TileId[,] groundLayer,
		WorldPosition position)
	{
		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);

		foreach (var neighbor in GroundConnectivityNeighbors(position))
		{
			if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
				continue;

			if (groundLayer[neighbor.X, neighbor.Y] != TileIds.Wall)
				continue;

			if (ceilingLayer[neighbor.X, neighbor.Y] == CeilingLayerTileIds.Solid)
				return true;
		}

		return false;
	}

	private static IEnumerable<WorldPosition> GroundConnectivityNeighbors(WorldPosition position)
	{
		yield return new WorldPosition(position.X + 1, position.Y);
		yield return new WorldPosition(position.X - 1, position.Y);
		yield return new WorldPosition(position.X, position.Y + 1);
		yield return new WorldPosition(position.X, position.Y - 1);
	}

	private static readonly WorldPosition CaveTestStart = new(2, 2);
	private static readonly WorldPosition CaveTestGoal = new(27, 17);

	private static WorldGenerationConfig CaveTestConfig() => new()
	{
		EnableCeilingLayer = true,
		MinWallBlobSize = 24,
		MinCaveAreaSize = 3,
		MinCaveEntrances = 1,
		MaxCaveEntrances = 1,
		MinEntranceWidth = 1,
		MaxEntranceWidth = 1,
		MaxCaveCount = 4,
		StartCeilingClearanceRadius = 0,
		ExtraWallStackChance = 0f
	};

	private static int[,] CreateEmptyRegionIndex(int width, int height)
	{
		var index = new int[width, height];

		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
			index[x, y] = -1;

		return index;
	}

	private static int[,] ApplyCarver(TileId[,] groundLayer, WorldGenerationConfig config, int seed = 9001)
	{
		var index = CreateEmptyRegionIndex(groundLayer.GetLength(0), groundLayer.GetLength(1));
		WallBlobCaveCarver.Apply(groundLayer, CaveTestStart, CaveTestGoal, seed, config, index);
		return index;
	}

	private static TileId[,] PlaceSyntheticCeiling(TileId[,] groundLayer, int[,] caveRegionIndex, WorldGenerationConfig config)
	{
		return CeilingLayerPlacer.Place(
			groundLayer,
			CaveTestStart,
			seed: 9001,
			config,
			caveRegionIndex).CeilingLayer;
	}

	private static void PaintGroundRect(TileId[,] tiles, int left, int top, int right, int bottom)
	{
		for (var y = top; y <= bottom; y++)
		for (var x = left; x <= right; x++)
			tiles[x, y] = TileIds.Ground;
	}

	private static void PaintWallRect(TileId[,] tiles, int left, int top, int right, int bottom)
	{
		for (var y = top; y <= bottom; y++)
		for (var x = left; x <= right; x++)
			tiles[x, y] = TileIds.Wall;
	}

	private static int CountCeilingCells(TileId[,] ceilingLayer)
	{
		var count = 0;
		for (var y = 0; y < ceilingLayer.GetLength(1); y++)
		for (var x = 0; x < ceilingLayer.GetLength(0); x++)
		{
			if (ceilingLayer[x, y] == CeilingLayerTileIds.Solid)
				count++;
		}

		return count;
	}

	private static int CountCarvedCaveCells(int[,] caveRegionIndex)
	{
		var count = 0;
		for (var y = 0; y < caveRegionIndex.GetLength(1); y++)
		for (var x = 0; x < caveRegionIndex.GetLength(0); x++)
		{
			if (caveRegionIndex[x, y] >= 0)
				count++;
		}

		return count;
	}

	private static int CountDistinctCaveRegions(int[,] caveRegionIndex)
	{
		var regions = new HashSet<int>();
		for (var y = 0; y < caveRegionIndex.GetLength(1); y++)
		for (var x = 0; x < caveRegionIndex.GetLength(0); x++)
		{
			if (caveRegionIndex[x, y] >= 0)
				regions.Add(caveRegionIndex[x, y]);
		}

		return regions.Count;
	}

	private static bool IsReachableFromStart(TileId[,] tiles, WorldPosition start, WorldPosition target)
	{
		var width = tiles.GetLength(0);
		var height = tiles.GetLength(1);
		var visited = new bool[width, height];
		var queue = new Queue<WorldPosition>();
		queue.Enqueue(start);
		visited[start.X, start.Y] = true;

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			if (current == target)
				return true;

			foreach (var neighbor in GroundConnectivityNeighbors(current))
			{
				if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
					continue;

				if (visited[neighbor.X, neighbor.Y])
					continue;

				if (tiles[neighbor.X, neighbor.Y] != TileIds.Ground)
					continue;

				visited[neighbor.X, neighbor.Y] = true;
				queue.Enqueue(neighbor);
			}
		}

		return false;
	}

	private static void EnclosedWallBlobCarvesInterior()
	{
		var groundLayer = CreateMapWithWallBlob(left: 10, top: 6, blobSize: 7);
		var config = CaveTestConfig();
		var caveRegionIndex = ApplyCarver(groundLayer, config);
		var interior = new WorldPosition(13, 9);

		TestAssert.Equal(TileIds.Ground.Id, groundLayer[interior.X, interior.Y].Id);
		TestAssert.True(caveRegionIndex[interior.X, interior.Y] >= 0);
		TestAssert.Equal(TileIds.Wall.Id, groundLayer[10, 9].Id);
	}

	private static void CarvedCaveConnectsToExterior()
	{
		var groundLayer = CreateMapWithConnectedWallBlob();
		var config = CaveTestConfig();
		ApplyCarver(groundLayer, config);
		var interior = new WorldPosition(13, 9);

		TestAssert.True(IsReachableFromStart(groundLayer, CaveTestStart, interior));
	}

	private static void TooSmallWallBlobRejected()
	{
		var groundLayer = CreateMapWithWallBlob(left: 10, top: 6, blobSize: 4);
		var config = CaveTestConfig();
		var caveRegionIndex = ApplyCarver(groundLayer, config);

		TestAssert.Equal(0, CountCarvedCaveCells(caveRegionIndex));
	}

	private static void MaxCaveCountLimitsCarvedCaves()
	{
		var groundLayer = CreateMapWithMultipleWallBlobs();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MaxCaveCount = 2,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 1234);

		TestAssert.Equal(2, CountDistinctCaveRegions(caveRegionIndex));
	}

	private static void CarvedCaveFloorGetsCeiling()
	{
		var groundLayer = CreateMapWithConnectedWallBlob();
		var config = CaveTestConfig();
		var caveRegionIndex = ApplyCarver(groundLayer, config);
		var ceilingLayer = PlaceSyntheticCeiling(groundLayer, caveRegionIndex, config);

		for (var y = 0; y < groundLayer.GetLength(1); y++)
		for (var x = 0; x < groundLayer.GetLength(0); x++)
		{
			if (caveRegionIndex[x, y] < 0)
				continue;

			TestAssert.Equal(CeilingLayerTileIds.Solid.Id, ceilingLayer[x, y].Id);
		}

		TestAssert.True(CountCeilingCells(ceilingLayer) > 0);
	}

	private static void CaveCarvingPreservesGroundPath()
	{
		var system = CreateSystem();

		for (var seed = 1; seed <= 10; seed++)
		{
			var map = system.Generator.Generate(new WorldGenerationConfig
			{
				Width = 32,
				Height = 24,
				Seed = seed,
				MaxCaveCount = 4
			});

			TestAssert.True(GroundConnectivity.HasGroundPath(map.Tiles, map.Start, map.Goal));
		}
	}

	private static void DisconnectedInteriorPocketNotCarved()
	{
		var groundLayer = CreateMapWithDisconnectedInteriorPocket();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MaxCaveCount = 1,
			MaxCaveAreaSize = 49,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 5555);
		var isolatedPocket = new WorldPosition(20, 8);

		TestAssert.Equal(-1, caveRegionIndex[isolatedPocket.X, isolatedPocket.Y]);
		TestAssert.Equal(TileIds.Wall.Id, groundLayer[isolatedPocket.X, isolatedPocket.Y].Id);
		TestAssert.True(CountCarvedCaveCells(caveRegionIndex) > 0);
	}

	private static void BorderTouchingBlobCarvesWithoutRemovingBorder()
	{
		var groundLayer = CreateMapWithBorderTouchingBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MaxCaveCount = 1,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = CreateEmptyRegionIndex(groundLayer.GetLength(0), groundLayer.GetLength(1));
		WallBlobCaveCarver.Apply(
			groundLayer,
			CaveTestStart,
			new WorldPosition(18, 12),
			seed: 7777,
			config,
			caveRegionIndex);
		var width = groundLayer.GetLength(0);
		var height = groundLayer.GetLength(1);

		for (var y = 0; y < height; y++)
			TestAssert.Equal(TileIds.Wall.Id, groundLayer[0, y].Id);

		for (var x = 0; x < width; x++)
		{
			TestAssert.Equal(TileIds.Wall.Id, groundLayer[x, 0].Id);
			TestAssert.Equal(TileIds.Wall.Id, groundLayer[x, height - 1].Id);
		}

		TestAssert.True(CountCarvedCaveCells(caveRegionIndex) > 0);
	}

	private static void MaxCaveAreaCapsHollowSize()
	{
		var groundLayer = CreateMapWithLargeWallBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MaxCaveCount = 1,
			MaxCaveAreaSize = 49,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 8888);

		TestAssert.Equal(49, CountCarvedCaveCells(caveRegionIndex));
	}

	private static void AllCarvedCaveFloorReachableFromStart()
	{
		var groundLayer = CreateMapWithConnectedWallBlob();
		var config = CaveTestConfig();
		var caveRegionIndex = ApplyCarver(groundLayer, config);

		for (var y = 0; y < groundLayer.GetLength(1); y++)
		for (var x = 0; x < groundLayer.GetLength(0); x++)
		{
			if (caveRegionIndex[x, y] < 0)
				continue;

			TestAssert.True(IsReachableFromStart(groundLayer, CaveTestStart, new WorldPosition(x, y)));
		}
	}

	private static TileId[,] CreateWallFilledMap(int width, int height)
	{
		var tiles = new TileId[width, height];

		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
			tiles[x, y] = TileIds.Wall;

		return tiles;
	}

	private static TileId[,] CreateBorderedMap(int width, int height)
	{
		var tiles = CreateWallFilledMap(width, height);

		for (var y = 1; y < height - 1; y++)
		for (var x = 1; x < width - 1; x++)
			tiles[x, y] = TileIds.Ground;

		return tiles;
	}

	private static TileId[,] CreateMapWithWallBlob(int left, int top, int blobSize)
	{
		var tiles = CreateBorderedMap(30, 20);
		PaintWallRect(tiles, left, top, left + blobSize - 1, top + blobSize - 1);
		return tiles;
	}

	private static TileId[,] CreateMapWithConnectedWallBlob()
	{
		var tiles = CreateMapWithWallBlob(left: 10, top: 6, blobSize: 7);
		PaintGroundRect(tiles, 8, 8, 9, 10);
		return tiles;
	}

	private static TileId[,] CreateMapWithMultipleWallBlobs()
	{
		var tiles = CreateBorderedMap(40, 24);
		PaintWallRect(tiles, 8, 5, 14, 11);
		PaintGroundRect(tiles, 6, 7, 7, 9);
		PaintWallRect(tiles, 18, 5, 24, 11);
		PaintGroundRect(tiles, 16, 7, 17, 9);
		PaintWallRect(tiles, 28, 5, 34, 11);
		PaintGroundRect(tiles, 26, 7, 27, 9);
		return tiles;
	}

	private static TileId[,] CreateMapWithDisconnectedInteriorPocket()
	{
		var tiles = CreateBorderedMap(30, 20);
		PaintWallRect(tiles, 8, 4, 22, 14);
		PaintGroundRect(tiles, 6, 8, 7, 10);

		for (var y = 5; y <= 13; y++)
			tiles[16, y] = TileIds.Wall;

		for (var x = 17; x <= 21; x++)
			tiles[x, 9] = TileIds.Wall;

		return tiles;
	}

	private static TileId[,] CreateMapWithBorderTouchingBlob()
	{
		var tiles = CreateBorderedMap(20, 14);
		PaintWallRect(tiles, 0, 4, 8, 12);
		for (var y = 7; y <= 9; y++)
		{
			tiles[9, y] = TileIds.Wall;
			tiles[10, y] = TileIds.Wall;
			tiles[11, y] = TileIds.Wall;
		}

		PaintGroundRect(tiles, 12, 7, 13, 9);
		return tiles;
	}

	private static TileId[,] CreateMapWithThickWallBlob()
	{
		var tiles = CreateBorderedMap(30, 20);
		PaintWallRect(tiles, 6, 5, 17, 15);
		PaintGroundRect(tiles, 4, 8, 5, 12);
		return tiles;
	}

	private static TileId[,] CreateMapWithDeepWallBlob()
	{
		var tiles = CreateWallFilledMap(30, 20);
		PaintGroundRect(tiles, 1, 1, 5, 5);
		PaintGroundRect(tiles, 4, 8, 5, 12);
		PaintGroundRect(tiles, 6, 18, 27, 18);
		PaintWallRect(tiles, 7, 4, 25, 16);
		return tiles;
	}

	private static int CountGroundThroatCells(TileId[,] groundLayer, int[,] caveRegionIndex)
	{
		var count = 0;
		for (var y = 0; y < groundLayer.GetLength(1); y++)
		for (var x = 0; x < groundLayer.GetLength(0); x++)
		{
			if (groundLayer[x, y] != TileIds.Ground)
				continue;

			if (caveRegionIndex[x, y] >= 0)
				continue;

			count++;
		}

		return count;
	}

	private static void ThickWallTunnelConnectsCaveToExterior()
	{
		var groundLayer = CreateMapWithThickWallBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MinEntranceDepth = 1,
			MaxEntranceDepth = 8,
			MaxCaveCount = 1,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 4242);
		var sampleInterior = new WorldPosition(10, 10);

		TestAssert.True(CountCarvedCaveCells(caveRegionIndex) > 0);
		TestAssert.True(IsReachableFromStart(groundLayer, CaveTestStart, sampleInterior));
	}

	private static void BorderBlobUsesTunnelWithoutRemovingBorder()
	{
		var groundLayer = CreateMapWithBorderTouchingBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MinEntranceDepth = 1,
			MaxEntranceDepth = 8,
			MaxCaveCount = 1,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = CreateEmptyRegionIndex(groundLayer.GetLength(0), groundLayer.GetLength(1));
		WallBlobCaveCarver.Apply(
			groundLayer,
			CaveTestStart,
			new WorldPosition(18, 12),
			seed: 7777,
			config,
			caveRegionIndex);

		TestAssert.True(CountCarvedCaveCells(caveRegionIndex) > 0);

		for (var y = 0; y < groundLayer.GetLength(1); y++)
			TestAssert.Equal(TileIds.Wall.Id, groundLayer[0, y].Id);

		var reachableInterior = false;
		for (var y = 0; y < groundLayer.GetLength(1); y++)
		for (var x = 0; x < groundLayer.GetLength(0); x++)
		{
			if (caveRegionIndex[x, y] < 0)
				continue;

			if (IsReachableFromStart(groundLayer, CaveTestStart, new WorldPosition(x, y)))
				reachableInterior = true;
		}

		TestAssert.True(reachableInterior);
	}

	private static void EntranceDepthLimitRejectsUnreachableCave()
	{
		var groundLayer = CreateMapWithDeepWallBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MinEntranceDepth = 1,
			MaxEntranceDepth = 3,
			MaxCaveCount = 1,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 9999);

		TestAssert.Equal(0, CountCarvedCaveCells(caveRegionIndex));
	}

	private static void TunnelThroatCellsAreNotCaveFloor()
	{
		var groundLayer = CreateMapWithThickWallBlob();
		var config = new WorldGenerationConfig
		{
			MinWallBlobSize = 24,
			MinCaveAreaSize = 3,
			MinCaveEntrances = 1,
			MaxCaveEntrances = 1,
			MinEntranceWidth = 1,
			MaxEntranceWidth = 1,
			MinEntranceDepth = 1,
			MaxEntranceDepth = 8,
			MaxCaveCount = 1,
			StartCeilingClearanceRadius = 0,
			ExtraWallStackChance = 0f
		};
		var caveRegionIndex = ApplyCarver(groundLayer, config, seed: 4242);

		TestAssert.True(CountGroundThroatCells(groundLayer, caveRegionIndex) > 0);
		TestAssert.True(CountCarvedCaveCells(caveRegionIndex) > 0);
	}

	private static TileId[,] CreateMapWithLargeWallBlob()
	{
		var tiles = CreateBorderedMap(30, 22);
		PaintWallRect(tiles, 6, 4, 20, 18);
		PaintGroundRect(tiles, 4, 10, 5, 12);
		return tiles;
	}
}
