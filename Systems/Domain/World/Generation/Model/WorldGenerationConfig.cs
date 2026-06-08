namespace Game.Systems.Domain.World.Generation.Model;

public sealed class WorldGenerationConfig
{
	public int Width { get; init; } = 64;
	public int Height { get; init; } = 48;
	public int Seed { get; init; } = 42;
	public float FillProbability { get; init; } = 0.48f;
	public int CellularAutomataIterations { get; init; } = 5;
	public int MaxAttempts { get; init; } = 50;
}
