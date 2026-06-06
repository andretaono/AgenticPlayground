using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.Raven;

public sealed class RavenConfig
{
	public float DirectSightRange { get; init; } = 192f;
	public float LongRangeScentRadius { get; init; } = 320f;
	public float ScentDetectionThreshold { get; init; } = 0.15f;
	public float CognitionCellSize { get; init; } = 32f;
	public int CognitionGridWidth { get; init; } = 64;
	public int CognitionGridHeight { get; init; } = 64;
	public float ObserveDistance { get; init; } = 64f;
	public int ObservePriority { get; init; } = 20;
	public int PatrolPriority { get; init; } = 10;

	public PerceptionConfig ToPerceptionConfig() => new()
	{
		DirectSightRange = DirectSightRange,
		LongRangeScentRadius = LongRangeScentRadius,
		ScentDetectionThreshold = ScentDetectionThreshold,
		CognitionCellSize = CognitionCellSize,
		CognitionGridWidth = CognitionGridWidth,
		CognitionGridHeight = CognitionGridHeight
	};

	public EnemyTacticalConfig ToTacticalConfig() => new()
	{
		IdPrefix = "raven",
		AttackRange = 0f,
		StalkMinDistance = ObserveDistance,
		StalkMaxDistance = ObserveDistance,
		PatrolPriority = PatrolPriority
	};
}
