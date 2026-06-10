using Game.Systems.Integration.Enemies;
using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Player;
using Game.Systems.Integration.Presentation;
using Game.Systems.Integration.TerrainMesh;
using Game.Systems.Integration.World;

namespace Game.Systems.Integration.Bootstrap;

public sealed class GameSessionConfig
{
	public WorldConfig World { get; init; } = WorldConfig.Default;
	public TerrainConfig Terrain { get; init; } = TerrainConfig.Default;
	public EnemySpawnConfig Enemies { get; init; } = EnemySpawnConfig.Default;
	public PolarBearConfig PolarBear { get; init; } = PolarBearConfig.Default;
	public TopDownCameraConfig Camera { get; init; } = TopDownCameraConfig.Default;
	public PlayerConfig Player { get; init; } = PlayerConfig.Default;
}

public static class GameSessionDefaults
{
	public static GameSessionConfig Default { get; } = new();
}
