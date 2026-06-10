namespace Game.Systems.Integration.Presentation.Ports;

public sealed class ActorVisualDescriptor
{
	public float BodyRadius { get; init; } = 0.4f;
	public float VerticalScale { get; init; } = 1f;
	public float ColorR { get; init; } = 0.9f;
	public float ColorG { get; init; } = 0.25f;
	public float ColorB { get; init; } = 0.2f;
	public bool IsPolarBear { get; init; }
}
