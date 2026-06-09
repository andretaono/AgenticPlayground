namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileBlock
{
	public int CellX { get; init; }
	public int CellZ { get; init; }
	public float MinX { get; init; }
	public float MinY { get; init; }
	public float MinZ { get; init; }
	public float MaxX { get; init; }
	public float MaxY { get; init; }
	public float MaxZ { get; init; }
	public SurfaceMaterialId Material { get; init; }
	public int CaveRegionId { get; init; } = -1;
}
