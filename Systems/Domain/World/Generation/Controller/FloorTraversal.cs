using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal static class FloorTraversal
{
	public static bool IsWalkableFloor(TileId tile) =>
		tile == TileIds.Ground || tile == TileIds.Water;

	public static IEnumerable<WorldPosition> GetNeighbors(WorldPosition position)
	{
		yield return new WorldPosition(position.X + 1, position.Y);
		yield return new WorldPosition(position.X - 1, position.Y);
		yield return new WorldPosition(position.X, position.Y + 1);
		yield return new WorldPosition(position.X, position.Y - 1);
	}
}
