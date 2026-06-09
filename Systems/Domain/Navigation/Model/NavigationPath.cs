using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.Navigation.Model;

public sealed class NavigationPath
{
	public NavigationPath(IReadOnlyList<WorldPosition> waypoints)
	{
		if (waypoints is null)
			throw new ArgumentNullException(nameof(waypoints));
		if (waypoints.Count == 0)
			throw new ArgumentException("Path must contain at least one waypoint.", nameof(waypoints));

		Waypoints = waypoints;
	}

	public IReadOnlyList<WorldPosition> Waypoints { get; }
}
