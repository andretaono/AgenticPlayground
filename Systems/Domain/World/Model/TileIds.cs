namespace Game.Systems.Domain.World.Model;

/// <summary>
/// Well-known tile identities used by generation and integration adapters.
/// </summary>
public static class TileIds
{
	public static readonly TileId Ground = new("ground");
	public static readonly TileId Wall = new("wall");
	public static readonly TileId Water = new("water");
}
