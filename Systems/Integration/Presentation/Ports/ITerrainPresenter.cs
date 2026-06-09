using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Systems.Integration.Presentation.Ports;

public interface ITerrainPresenter
{
	void SyncTerrain(
		GeneratedWorldMap map,
		TerrainBuildResult result,
		TileHeightModifierSettings settings,
		TileSurfaceMeshSettings? surfaceSettings = null);
}
