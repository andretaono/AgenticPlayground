using Game.Systems.Domain.Navigation.Controller;
using Game.Systems.Domain.Navigation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Testing;

namespace Game.Systems.Domain.Navigation.Tests;

public sealed class NavigationTests : ITestSuite
{
	public const int GroundCost = 1;
	public const int WaterCost = 4;
	public const int BlockedCost = 0;

	public string Name => "unit/navigation";

	public void Register(TestRegistry registry)
	{
		registry.Add(Name, "finds straight corridor path", FindsStraightCorridorPath);
		registry.Add(Name, "routes around a wall", RoutesAroundWall);
		registry.Add(Name, "prefers ground over water when cheaper", PrefersGroundOverWater);
		registry.Add(Name, "crosses water when it is the only route", CrossesWaterWhenOnlyRoute);
		registry.Add(Name, "returns null when no path exists", ReturnsNullWhenNoPath);
	}

	private static void FindsStraightCorridorPath()
	{
		var grid = CreateGrid(
			"GGG",
			"GGG",
			"GGG");

		var path = new AStarGridPathfinder().TryFindPath(grid, Pos(0, 1), Pos(2, 1));

		TestAssert.True(path is not null);
		TestAssert.Equal(3, path!.Waypoints.Count);
		TestAssert.True(path.Waypoints[0] == Pos(0, 1));
		TestAssert.True(path.Waypoints[^1] == Pos(2, 1));
	}

	private static void RoutesAroundWall()
	{
		var grid = CreateGrid(
			"GGG",
			"GWG",
			"GGG");

		var path = new AStarGridPathfinder().TryFindPath(grid, Pos(0, 1), Pos(2, 1));

		TestAssert.True(path is not null);
		TestAssert.True(path!.Waypoints.Count >= 5);
		foreach (var waypoint in path.Waypoints)
			TestAssert.True(grid.IsWalkable(waypoint.X, waypoint.Y));
	}

	private static void PrefersGroundOverWater()
	{
		var grid = CreateGrid(
			"GGG",
			"GWG",
			"GGG");

		var path = new AStarGridPathfinder().TryFindPath(grid, Pos(0, 0), Pos(2, 2));

		TestAssert.True(path is not null);
		TestAssert.True(path!.Waypoints.All(tile => grid.GetMoveCost(tile.X, tile.Y) == GroundCost));
	}

	private static void CrossesWaterWhenOnlyRoute()
	{
		var grid = CreateGrid(
			"GWG",
			"GWG",
			"GWG");

		var path = new AStarGridPathfinder().TryFindPath(grid, Pos(0, 0), Pos(2, 2));

		TestAssert.True(path is not null);
		TestAssert.True(path!.Waypoints.Any(tile => grid.GetMoveCost(tile.X, tile.Y) == WaterCost));
	}

	private static void ReturnsNullWhenNoPath()
	{
		var grid = CreateGrid(
			"GXG",
			"XXX",
			"GXG");

		var path = new AStarGridPathfinder().TryFindPath(grid, Pos(0, 0), Pos(2, 2));

		TestAssert.True(path is null);
	}

	internal static NavigationGrid CreateGrid(params string[] rows)
	{
		var width = rows[0].Length;
		var height = rows.Length;
		var costs = new int[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				costs[x, y] = rows[y][x] switch
				{
					'G' => GroundCost,
					'W' => WaterCost,
					'X' => BlockedCost,
					_ => BlockedCost
				};
			}
		}

		return new NavigationGrid(width, height, costs);
	}

	private static WorldPosition Pos(int x, int y) => new(x, y);
}
