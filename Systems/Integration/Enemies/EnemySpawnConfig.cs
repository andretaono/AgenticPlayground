namespace Game.Systems.Integration.Enemies;

public sealed class EnemySpawnConfig
{
	public static EnemySpawnConfig Default { get; } = new();

	public int MinPolarBearCount { get; init; } = 1;
	public int MaxPolarBearCount { get; init; } = 3;
}
