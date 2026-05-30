namespace Game.Systems.Domain.World.Model;

public readonly record struct WorldPosition(int X, int Y);

public readonly record struct TileId(string Id)
{
	public override string ToString() => Id;
}

public readonly record struct WorldTile(WorldPosition Position, TileId Id);
