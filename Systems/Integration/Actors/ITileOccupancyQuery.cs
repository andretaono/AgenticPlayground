using Game.Systems.Domain.World.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Actors;

public interface ITileOccupancyQuery
{
	bool IsTileOccupied(WorldPosition tile, EntityId excludeEntityId);
}
