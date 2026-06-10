namespace Game.Systems.Integration.TerrainMesh;

public sealed class TerrainConfig
{
	public static TerrainConfig Default { get; } = new();

	public float WorldUnitsPerTile { get; init; } = 1f;
	public float HeightScale { get; init; } = 1f;
	public TileHeightModifierSettings Heights { get; init; } = new();
	public TileSurfaceMeshSettings SurfaceMesh { get; init; } = new();
}
