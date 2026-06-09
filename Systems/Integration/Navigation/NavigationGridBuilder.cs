using Game.Systems.Domain.Navigation.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.Navigation;

public static class NavigationGridBuilder
{
	public const int GroundMoveCost = 1;
	public const int WaterMoveCost = 4;
	public const int BlockedMoveCost = 0;

	public static NavigationGrid Build(
		InMemoryWorldDataSource worldData,
		ITileRulesProvider tileRulesProvider)
	{
		if (worldData is null)
			throw new ArgumentNullException(nameof(worldData));
		if (tileRulesProvider is null)
			throw new ArgumentNullException(nameof(tileRulesProvider));

		var map = worldData.LoadMap();
		var width = worldData.Width;
		var height = worldData.Height;
		var costs = new int[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var rules = tileRulesProvider.GetRules(map[x, y]);
				costs[x, y] = rules.HasFlag(TileRules.BlocksMovement)
					? BlockedMoveCost
					: rules.HasFlag(TileRules.Swimable)
						? WaterMoveCost
						: GroundMoveCost;
			}
		}

		return new NavigationGrid(width, height, costs);
	}
}
