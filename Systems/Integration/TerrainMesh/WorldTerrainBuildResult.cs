using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.TerrainMesh;

public sealed record WorldTerrainBuildResult(
	Heightmap Heightmap,
	TerrainMeshData Mesh,
	IReadOnlyList<TileId> TileOverlay);
