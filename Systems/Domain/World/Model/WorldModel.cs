namespace Game.Systems.Domain.World.Model;

/// <summary>
/// Lightweight world primitives. TileId is a simple identifier separating data from presentation/rules.
/// </summary>
public readonly record struct WorldPosition(int X, int Y)
{
	public static WorldPosition FromWorldUnits(float worldX, float worldY, int tileSize = 1) =>
		new((int)MathF.Floor(worldX / tileSize), (int)MathF.Floor(worldY / tileSize));
}

public readonly record struct TileId(string Id)
{
    public static readonly TileId Empty = new(string.Empty);
    public override string ToString() => Id;
}

public readonly record struct WorldTile(WorldPosition Position, TileId Id);