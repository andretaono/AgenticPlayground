using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Domain.World.Generation.Controller;

internal sealed class CeilingPlacementResult
{
	public CeilingPlacementResult(TileId[,] ceilingLayer, int[,] caveRegionIndex)
	{
		CeilingLayer = ceilingLayer ?? throw new ArgumentNullException(nameof(ceilingLayer));
		CaveRegionIndex = caveRegionIndex ?? throw new ArgumentNullException(nameof(caveRegionIndex));
	}

	public TileId[,] CeilingLayer { get; }

	public int[,] CaveRegionIndex { get; }
}
