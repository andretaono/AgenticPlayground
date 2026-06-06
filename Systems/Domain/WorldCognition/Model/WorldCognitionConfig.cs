namespace Game.Systems.Domain.WorldCognition.Model;

public sealed class WorldCognitionConfig
{
	public float CellSize { get; init; } = 32f;
	public int GridWidth { get; init; } = 256;
	public int GridHeight { get; init; } = 256;
	public int QueryRadiusCells { get; init; } = 3;
	public float PresenceDecayPerSecond { get; init; } = 0.995f;
	public float DisturbanceDecayPerSecond { get; init; } = 0.999f;
	public float AffinityDecayPerSecond { get; init; } = 0.9995f;
	public float DerivedRecalculationIntervalSeconds { get; init; } = 1f;
	public float AwarenessPresenceWeight { get; init; } = 0.7f;
	public float AwarenessDisturbanceWeight { get; init; } = 0.3f;
}
