using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Interfaces;

namespace Game.Systems.Domain.World;

/// <summary>
/// Core world system: deterministic, bounds-safe queries. Pure C# core logic.
/// Uses TileId values only; presentation and gameplay rules are provided by adapters.
/// </summary>
public sealed class WorldSystem : IWorldSystem
{
    private readonly TileId[,] _map;
    public int Width { get; }
    public int Height { get; }

    public WorldSystem(IWorldDataSource dataSource)
    {
        var ds = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _map = ds.LoadMap() ?? throw new InvalidOperationException("Data source returned null map");
        Width = _map.GetLength(0);
        Height = _map.GetLength(1);
    }

    public bool IsInBounds(WorldPosition pos) =>
        pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;

    public TileId GetTileId(WorldPosition pos)
    {
        if (!IsInBounds(pos)) return TileId.Empty;
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
        for (int dx = -radius; dx <= radius; dx++)
        for (int dy = -radius; dy <= radius; dy++)
        {
            var pos = new WorldPosition(center.X + dx, center.Y + dy);
            if (TryGetTile(pos, out var tile))
                list.Add(tile);
        }
        return list;
    }
}