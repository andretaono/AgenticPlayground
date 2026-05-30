namespace Game.Systems.Domain.World.Model;

public sealed class WorldCoordinateConverter
{
	public WorldPosition ToTilePosition(float worldX, float worldY, int tileSize)
	{
		if (tileSize <= 0) throw new ArgumentOutOfRangeException(nameof(tileSize));
		return new WorldPosition(
			(int)MathF.Floor(worldX / tileSize),
			(int)MathF.Floor(worldY / tileSize));
	}
}
