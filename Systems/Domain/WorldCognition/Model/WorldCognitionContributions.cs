namespace Game.Systems.Domain.WorldCognition.Model;

/// <summary>
/// Documented contribution amounts from the World Cognition spec.
/// </summary>
public static class WorldCognitionContributions
{
	public static class Presence
	{
		public const float Movement = 0.25f;
		public const float Sprinting = 0.5f;
		public const float Resting = 5f;
		public const float Campfire = 15f;
		public const float ShapeChange = 10f;
	}

	public static class Disturbance
	{
		public const float CombatHit = 1f;
		public const float EnemyKill = 10f;
		public const float EliteKill = 20f;
		public const float BossKill = 50f;
		public const float TerrainDestruction = 5f;
		public const float MajorTransformation = 15f;
	}

	public static class Affinity
	{
		public const float MeleeKillBear = 5f;
		public const float HeavyAttackBear = 1f;
		public const float FlightRaven = 2f;
		public const float DiscoveryRaven = 5f;
		public const float SwimSeal = 1f;
		public const float UnderwaterTraversalSeal = 5f;
	}
}
