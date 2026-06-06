using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Integration.Enemies.Common.Advantage;
using Game.Systems.Integration.Enemies.Common.Config;
using Game.Systems.Integration.Enemies.Common.Perception;

namespace Game.Systems.Integration.Enemies.PolarBear;

/// <summary>
/// Attack advantage is OR-based: wounded prey, high presence (rest/camp), or awareness ≥ Tracked.
/// </summary>
public sealed class PolarBearConfig
{
	public float AttackRange { get; init; } = 2.5f;
	public float DirectSightRange { get; init; } = 96f;
	public float LongRangeScentRadius { get; init; } = 480f;
	public float ScentDetectionThreshold { get; init; } = 0.5f;
	public float StalkMinDistance { get; init; } = 12f;
	public float StalkMaxDistance { get; init; } = 48f;
	public float VulnerableHealthThreshold { get; init; } = 60f;
	public float VulnerablePresenceThreshold { get; init; } = 8f;
	public float CognitionCellSize { get; init; } = 32f;
	public int CognitionGridWidth { get; init; } = 64;
	public int CognitionGridHeight { get; init; } = 64;
	public float MeleeBasePower { get; init; } = 35f;

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
		IdPrefix = "polar-bear",
		AttackRange = AttackRange,
		StalkMinDistance = StalkMinDistance,
		StalkMaxDistance = StalkMaxDistance
	};

	public IReadOnlyList<IAttackAdvantageRule> CreateAdvantageRules() =>
	[
		new LowHealthAdvantageRule(VulnerableHealthThreshold),
		new HighPresenceAdvantageRule(VulnerablePresenceThreshold),
		new AwarenessAdvantageRule(AwarenessState.Tracked)
	];
}
