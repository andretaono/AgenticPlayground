using Game.Systems.Domain.World.Controller;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;

namespace Game.Systems.Domain.World;

/// <summary>
/// Root orchestrator for deterministic world queries.
/// </summary>
public sealed class WorldSystem : IWorldSystem
{
	private readonly WorldQueryController _queries;

	public WorldSystem(IWorldDataSource dataSource)
	{
		_queries = new WorldQueryController(dataSource);
	}

	public bool IsInBounds(WorldPosition pos) => _queries.IsInBounds(pos);

	public TileId GetTileId(WorldPosition pos) => _queries.GetTileId(pos);

	public bool TryGetTile(WorldPosition pos, out WorldTile tile) => _queries.TryGetTile(pos, out tile);

	public IReadOnlyList<WorldTile> GetNeighborhood(WorldPosition center, int radius) =>
		_queries.GetNeighborhood(center, radius);
}
