using Game.Systems.Domain.TerrainMesh.Model;

namespace Game.Systems.Domain.TerrainMesh.Ports;

public interface IHeightmapSampler
{
	float Sample(Heightmap heightmap, int x, int y);
	float SampleBilinear(Heightmap heightmap, float worldX, float worldZ);
}
