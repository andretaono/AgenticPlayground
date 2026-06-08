namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileHeightModifierSettings
{
	public float GroundHeight { get; init; } = 0f;
	public float WallHeight { get; init; } = 1f;
	public float WaterHeight { get; init; } = -1f;
	public float BevelInset { get; init; } = 0.3f;
	public int BevelSegments { get; init; } = 4;
}
