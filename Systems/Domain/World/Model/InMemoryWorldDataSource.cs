using Game.Systems.Domain.World.Interfaces;

namespace Game.Systems.Domain.World.Model;

/// <summary>
/// Simple in-memory data source for examples and small maps.
/// </summary>
public sealed class InMemoryWorldDataSource : IWorldDataSource
{
    private readonly TileId[,] _map;

    public InMemoryWorldDataSource(TileId[,] map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        Width = _map.GetLength(0);
        Height = _map.GetLength(1);
    }

    public int Width { get; }
    public int Height { get; }

    public TileId[,] LoadMap()
    {
        var copy = new TileId[Width, Height];
        for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                copy[x, y] = _map[x, y];
        return copy;
    }
}