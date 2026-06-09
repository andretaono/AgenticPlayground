using Game.Systems.Domain.TerrainMesh.Model;

namespace Game.Systems.Integration.TerrainMesh;

public sealed record WorldTerrainMapping(
	int Seed,
	float WorldUnitsPerTile,
	TerrainMeshConfig TerrainConfig,
	TileHeightModifierSettings? ModifierSettings = null,
	TileSurfaceMeshSettings? SurfaceSettings = null);
