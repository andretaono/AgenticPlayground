using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.TerrainMesh;

namespace Game.Systems.Integration.Presentation;

public sealed class NullTerrainPresenter : ITerrainPresenter
{
	public void SyncTerrainMesh(WorldTerrainBuildResult buildResult) => _ = buildResult;
}
