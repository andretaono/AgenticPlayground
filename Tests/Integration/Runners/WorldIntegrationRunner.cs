using Game.Systems.Domain.World;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;
using Game.Systems.Integration.Adapters;

namespace Game.Tests.Integration.Runners;

public sealed class WorldIntegrationRunner
{
	public WorldIntegrationResult Run()
	{
		const int width = 10;
		const int height = 6;
		var map = CreateDemoMap(width, height);

		IWorldDataSource dataSource = new InMemoryWorldDataSource(map);
		var world = new WorldSystem(dataSource);
		var rules = new DefaultTileRulesProvider();

		var center = new WorldPosition(5, 2);
		var outOfBounds = new WorldPosition(-1, -1);
		var neighborhood = world.GetNeighborhood(center, radius: 1);
		var centerTileId = world.GetTileId(center);
		var waterTiles = neighborhood.Count(tile => tile.Id.Id == "water");
		var wallTiles = neighborhood.Count(tile => rules.GetRules(tile.Id).HasFlag(TileRules.BlocksMovement));

		return new WorldIntegrationResult(
			CenterTileId: centerTileId.Id,
			IsOutOfBounds: !world.IsInBounds(outOfBounds),
			NeighborhoodCount: neighborhood.Count,
			WaterTilesInNeighborhood: waterTiles,
			WallTilesInNeighborhood: wallTiles);
	}

	internal static TileId[,] CreateDemoMap(int width, int height)
	{
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

		map[4, 2] = new TileId("water");
		map[5, 2] = new TileId("water");
		map[4, 3] = new TileId("water");

		return map;
	}
}

public sealed record WorldIntegrationResult(
	string CenterTileId,
	bool IsOutOfBounds,
	int NeighborhoodCount,
	int WaterTilesInNeighborhood,
	int WallTilesInNeighborhood);
