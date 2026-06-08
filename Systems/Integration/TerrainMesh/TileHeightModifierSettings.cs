namespace Game.Systems.Integration.TerrainMesh;

public sealed class TileHeightModifierSettings
{
	public float GroundHeight { get; init; } = 0f;
	public float WallHeight { get; init; } = 1f;
	public float WaterHeight { get; init; } = -1f;
}
