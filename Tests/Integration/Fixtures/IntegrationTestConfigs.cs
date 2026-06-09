using Game.Systems.Integration.Enemies.PolarBear;
using Game.Systems.Integration.Player;

namespace Game.Tests.Integration.Fixtures;

/// <summary>
/// Fixed configs for integration tests. Values here are independent of production defaults
/// so tuning gameplay settings does not break the test suite.
/// </summary>
internal static class IntegrationTestConfigs
{
	public const float PlayerGroundSpeed = 5f;
	public const float PlayerSwimSpeed = 3f;
	public const float BearGroundSpeed = 5f;
	public const float BearSwimSpeed = 3f;

	public const float PerEntitySpeedTestPlayerGround = 4f;
	public const float PerEntitySpeedTestBearGround = 2f;

	public static PlayerConfig PlayerMovement() => new()
	{
		GroundSpeed = PlayerGroundSpeed,
		SwimSpeed = PlayerSwimSpeed
	};

	public static PolarBearConfig PolarBearBehaviourScenario() => new()
	{
		AttackRange = 2.5f,
		DirectSightRange = 96f,
		LongRangeScentRadius = 480f,
		ScentDetectionThreshold = 0.2f,
		StalkMinDistance = 2f,
		StalkMaxDistance = 48f,
		VulnerableHealthThreshold = 60f,
		VulnerablePresenceThreshold = 8f,
		CognitionCellSize = 32f,
		CognitionGridWidth = 64,
		CognitionGridHeight = 64,
		MeleeBasePower = 35f,
		GroundSpeed = BearGroundSpeed,
		SwimSpeed = BearSwimSpeed
	};

	public static PolarBearConfig PolarBearNavigationScenario() => new()
	{
		ScentDetectionThreshold = 0.01f,
		DirectSightRange = 96f,
		LongRangeScentRadius = 96f,
		StalkMinDistance = 2f,
		StalkMaxDistance = 48f,
		AttackRange = 2.5f,
		GroundSpeed = BearGroundSpeed,
		SwimSpeed = BearSwimSpeed
	};
}
