using Game.Systems.Integration.TerrainMesh;

namespace Game.Systems.Integration.Presentation.Ports;

public interface ITerrainPresenter
{
	void SyncTerrainMesh(WorldTerrainBuildResult buildResult);
}
