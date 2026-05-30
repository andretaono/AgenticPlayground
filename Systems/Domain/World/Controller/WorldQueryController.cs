using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;

namespace Game.Systems.Domain.World.Controller;

internal sealed class WorldQueryController
{
	private readonly TileId[,] _map;

	public int Width { get; }
	public int Height { get; }

	public WorldQueryController(IWorldDataSource dataSource)
	{
		var source = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
		_map = source.LoadMap() ?? throw new InvalidOperationException("Data source returned null map");
		Width = _map.GetLength(0);
		Height = _map.GetLength(1);
	}

	public bool IsInBounds(WorldPosition pos) =>
		pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;

	public TileId GetTileId(WorldPosition pos)
	{
		if (!IsInBounds(pos)) return new TileId(string.Empty);
		return _map[pos.X, pos.Y];
	}

	public bool TryGetTile(WorldPosition pos, out WorldTile tile)
	{
		if (!IsInBounds(pos))
		{
			tile = default;
			return false;
		}

		tile = new WorldTile(pos, _map[pos.X, pos.Y]);
		return true;
	}

	public IReadOnlyList<WorldTile> GetNeighborhood(WorldPosition center, int radius)
	{
		if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius));

		var list = new List<WorldTile>();
		for (var dx = -radius; dx <= radius; dx++)
		for (var dy = -radius; dy <= radius; dy++)
		{
			var pos = new WorldPosition(center.X + dx, center.Y + dy);
			if (TryGetTile(pos, out var tile))
				list.Add(tile);
		}

		return list;
	}
}
