using Game.Systems.Domain.TerrainMesh.Model;

namespace Game.Systems.Domain.TerrainMesh.Ports;

public interface IHeightmapGenerator
{
	Heightmap Generate(int seed, int width, int height, TerrainMeshConfig config);
}
