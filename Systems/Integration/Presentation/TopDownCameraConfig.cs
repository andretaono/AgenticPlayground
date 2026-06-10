namespace Game.Systems.Integration.Presentation;

public sealed class TopDownCameraConfig
{
	public static TopDownCameraConfig Default { get; } = new();

	public float OrbitDistance { get; init; } = 16f;
	public float PitchDegrees { get; init; } = 52f;
	public float LookHeight { get; init; } = 0.75f;
	public float LookAhead { get; init; } = 0.25f;
	public float YawSmoothTime { get; init; } = 0.15f;
	public float PositionSmoothTime { get; init; } = 0.1f;
	public float RotationSmoothTime { get; init; } = 0.08f;
}
