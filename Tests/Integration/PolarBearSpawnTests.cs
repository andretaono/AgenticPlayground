using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Testing;
using Game.Systems.Integration.Enemies.PolarBear;

namespace Game.Tests.Integration;

public sealed class PolarBearSpawnTests : ITestSuite
{
	public string Name => "polar-bear-spawn";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "picks deterministic count between min and max", () =>
		{
			var tiles = CreateMap(
				"GGG",
				"GGG",
				"GGG");

			var first = PolarBearSpawnPlacer.Place(tiles, Start(0, 0), Goal(2, 2), seed: 42, minCount: 1, maxCount: 3);
			var second = PolarBearSpawnPlacer.Place(tiles, Start(0, 0), Goal(2, 2), seed: 42, minCount: 1, maxCount: 3);

			TestAssert.Equal(first.Count, second.Count);
			TestAssert.True(first.Count >= 1);
			TestAssert.True(first.Count <= 3);
		});

		registry.Add(Name, "places only on available ground tiles", () =>
		{
			var tiles = CreateMap(
				"GWG",
				"GGG",
				"GGG");

			var spawns = PolarBearSpawnPlacer.Place(tiles, Start(0, 0), Goal(2, 0), seed: 7, minCount: 10, maxCount: 10);

			TestAssert.True(spawns.Count > 0);
			foreach (var spawn in spawns)
			{
				TestAssert.Equal(TileIds.Ground, tiles[spawn.X, spawn.Y]);
				TestAssert.False(spawn == Start(0, 0));
				TestAssert.False(spawn == Goal(2, 0));
			}
		});

		registry.Add(Name, "skips spawns when no eligible ground remains", () =>
		{
			var tiles = CreateMap("G");
			var spawns = PolarBearSpawnPlacer.Place(tiles, Start(0, 0), Goal(0, 0), seed: 9, minCount: 1, maxCount: 3);

			TestAssert.Equal(0, spawns.Count);
		});

		registry.Add(Name, "caps placement at available tile count", () =>
		{
			var tiles = CreateMap(
				"GGG",
				"GGG",
				"GGG");

			var spawns = PolarBearSpawnPlacer.Place(tiles, Start(0, 0), Goal(2, 2), seed: 11, minCount: 10, maxCount: 10);

			TestAssert.Equal(7, spawns.Count);
		});
	}

	private static TileId[,] CreateMap(params string[] rows)
	{
		var width = rows[0].Length;
		var height = rows.Length;
		var tiles = new TileId[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				tiles[x, y] = rows[y][x] switch
				{
					'G' => TileIds.Ground,
					'W' => TileIds.Water,
					_ => TileIds.Wall
				};
			}
		}

		return tiles;
	}

	private static WorldPosition Start(int x, int y) => new(x, y);

	private static WorldPosition Goal(int x, int y) => new(x, y);
}
