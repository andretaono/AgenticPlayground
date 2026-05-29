using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Interfaces;

public interface IWorldDataSource
{
    int Width { get; }
    int Height { get; }
    TileId[,] LoadMap();
}