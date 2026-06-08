using Game.Systems.Domain.TerrainMesh.Controller;
using Game.Systems.Domain.TerrainMesh.Ports;

namespace Game.Systems.Domain.TerrainMesh;

/// <summary>
/// Root orchestrator: wires heightmap generation, sampling, and mesh building.
/// </summary>
public sealed class TerrainMeshSystem : ITerrainMeshSystem
{
	public IHeightmapGenerator Generator { get; }
	public IHeightmapSampler Sampler { get; }
	public ITerrainMeshBuilder MeshBuilder { get; }

	public TerrainMeshSystem()
	{
		var sampler = new HeightmapSamplerController();

		Generator = new HeightmapGeneratorController();
		Sampler = sampler;
		MeshBuilder = new TerrainMeshBuilderController(sampler);
	}
}
