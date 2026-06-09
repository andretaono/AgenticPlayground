using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Model;

public sealed class CarvedCaveInfo
{
	public CarvedCaveInfo(int regionId, int floorSize, WorldPosition outermostEntrance)
	{
		RegionId = regionId;
		FloorSize = floorSize;
		OutermostEntrance = outermostEntrance;
	}

	public int RegionId { get; }

	public int FloorSize { get; }

	/// <summary>Tunnel cell on the accessible side of the cave entrance.</summary>
	public WorldPosition OutermostEntrance { get; }
}
