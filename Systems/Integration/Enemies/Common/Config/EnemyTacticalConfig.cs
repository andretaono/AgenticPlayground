namespace Game.Systems.Integration.Enemies.Common.Config;

/// <summary>
/// Distance bands and behaviour priorities for predator-style enemies.
/// For melee predators, StalkMinDistance should be less than or equal to AttackRange.
/// </summary>
public sealed class EnemyTacticalConfig
{
	public string IdPrefix { get; init; } = "enemy";
	public float AttackRange { get; init; } = 2.5f;
	public float StalkMinDistance { get; init; } = 12f;
	public float StalkMaxDistance { get; init; } = 48f;
	public float PatrolTurnDistance { get; init; } = 48f;
	public int PatrolPriority { get; init; } = 10;
	public int TrackPriority { get; init; } = 20;
	public int StalkPriority { get; init; } = 30;
	public int AttackPriority { get; init; } = 40;
}
