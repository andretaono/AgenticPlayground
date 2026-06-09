namespace Game.Systems.Integration.Enemies.Common.Config;

/// <summary>
/// Distance bands and behaviour priorities for predator-style enemies.
/// Attack reach comes from the combat ability targeting rule, not this config.
/// </summary>
public sealed class EnemyTacticalConfig
{
	public string IdPrefix { get; init; } = "enemy";
	public float StalkMinDistance { get; init; } = 12f;
	public float StalkMaxDistance { get; init; } = 48f;
	public float PatrolTurnDistance { get; init; } = 48f;
	public int PatrolPriority { get; init; } = 10;
	public int TrackPriority { get; init; } = 20;
	public int StalkPriority { get; init; } = 30;
	public int AttackPriority { get; init; } = 40;
}
