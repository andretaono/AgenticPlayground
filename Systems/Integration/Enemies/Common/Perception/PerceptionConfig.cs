namespace Game.Systems.Integration.Enemies.Common.Perception;

public sealed class PerceptionConfig
{
	public float DirectSightRange { get; init; } = 96f;
	public float LongRangeScentRadius { get; init; } = 480f;
	public float ScentDetectionThreshold { get; init; } = 0.5f;
	public float CognitionCellSize { get; init; } = 32f;
	public int CognitionGridWidth { get; init; } = 64;
	public int CognitionGridHeight { get; init; } = 64;
}
