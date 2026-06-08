namespace Game.Systems.Domain.TerrainMesh.Ports;

public interface ITerrainMeshSystem
{
	IHeightmapGenerator Generator { get; }
	IHeightmapSampler Sampler { get; }
	ITerrainMeshBuilder MeshBuilder { get; }
}
