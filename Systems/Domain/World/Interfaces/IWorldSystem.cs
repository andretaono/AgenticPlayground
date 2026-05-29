using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Interfaces;

public interface IWorldSystem
{
    bool IsInBounds(WorldPosition pos);
    TileId GetTileId(WorldPosition pos);
    IReadOnlyList<WorldTile> GetNeighborhood(WorldPosition center, int radius);
    bool TryGetTile(WorldPosition pos, out WorldTile tile);
}