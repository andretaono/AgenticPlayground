using Game.Systems.Domain.Navigation.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.Navigation.Ports;

public interface IGridPathfinder
{
	NavigationPath? TryFindPath(
		NavigationGrid grid,
		WorldPosition start,
		WorldPosition goal,
		Func<WorldPosition, bool>? isTileBlocked = null);
}
