using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.Adapters;

public interface ITileRulesProvider
{
	TileRules GetRules(TileId id);
}
