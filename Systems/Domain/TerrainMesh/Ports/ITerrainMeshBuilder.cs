using Game.Systems.Domain.TerrainMesh.Model;

namespace Game.Systems.Domain.TerrainMesh.Ports;

public interface ITerrainMeshBuilder
{
	TerrainMeshData Build(Heightmap heightmap, TerrainMeshConfig config);
}
