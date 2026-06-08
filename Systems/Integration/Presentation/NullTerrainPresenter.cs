using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Systems.Integration.Presentation;

public sealed class NullTerrainPresenter : ITerrainPresenter
{
	public void SyncTerrain(
		GeneratedWorldMap map,
		TerrainBuildResult result,
		TileHeightModifierSettings settings)
	{
		_ = map;
		_ = result;
		_ = settings;
	}
}
