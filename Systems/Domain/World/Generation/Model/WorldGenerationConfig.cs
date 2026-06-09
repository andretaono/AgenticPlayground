namespace Game.Systems.Domain.World.Generation.Model;

public sealed class WorldGenerationConfig
{
	public int Width { get; init; } = 64;
	public int Height { get; init; } = 48;
	public int Seed { get; init; } = 42;
	public float FillProbability { get; init; } = 0.48f;
	public int CellularAutomataIterations { get; init; } = 5;
	public int MaxAttempts { get; init; } = 50;
	public int WaterPoolAttempts { get; init; } = 12;
	public int WaterPoolMaxSize { get; init; } = 5;
	public bool EnableCeilingLayer { get; init; } = true;
	public int MinWallBlobSize { get; init; } = 25;
	public int MinCaveAreaSize { get; init; } = 3;
	public int MaxCaveAreaSize { get; init; } = 49;
	public int MinCaveEntrances { get; init; } = 1;
	public int MaxCaveEntrances { get; init; } = 2;
	public int MinEntranceWidth { get; init; } = 1;
	public int MaxEntranceWidth { get; init; } = 3;
	public int MinEntranceDepth { get; init; } = 1;
	public int MaxEntranceDepth { get; init; } = 8;
	public int MaxCaveCount { get; init; } = 4;
	public int MaxCavesPerBlob { get; init; } = 1;
	public float ExtraWallStackChance { get; init; } = 0.15f;
	public float ExtraWallStackClusterChance { get; init; } = 0.75f;
	public int ExtraWallStackGrowPasses { get; init; } = 4;
	public int StartCeilingClearanceRadius { get; init; } = 4;
}
