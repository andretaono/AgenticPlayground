using Game.Systems.Domain.WorldCognition;
using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Tests.Integration.Runners;

public sealed class WorldCognitionIntegrationRunner
{
	public WorldCognitionIntegrationResult Run()
	{
		var config = new WorldCognitionConfig
		{
			GridWidth = 32,
			GridHeight = 32,
			CellSize = 32f,
			QueryRadiusCells = 2
		};

		var cognition = new WorldCognitionSystem(config);
		var position = new Vector2(64f, 64f);

		cognition.Cognition.AddPresence(position, WorldCognitionContributions.Presence.Movement);
		cognition.Cognition.AddPresence(position, WorldCognitionContributions.Presence.Campfire);
		cognition.Cognition.AddDisturbance(position, WorldCognitionContributions.Disturbance.EnemyKill);
		cognition.Cognition.AddAffinity(position, AffinityType.Bear, WorldCognitionContributions.Affinity.MeleeKillBear);
		cognition.Cognition.AddAffinity(position, AffinityType.Raven, WorldCognitionContributions.Affinity.DiscoveryRaven);

		var cellAfterActivity = cognition.Cognition.GetCell(position);
		var awareness = cognition.Cognition.GetAwareness(position);
		var mood = cognition.Cognition.GetRegionalMood(position);
		var interest = cognition.Cognition.GetEcologicalInterest(position);

		const float deltaTime = 1f;
		for (var second = 1; second <= 30; second++)
			cognition.Simulation.AdvanceSimulation(deltaTime);

		var cellAfterDecay = cognition.Cognition.GetCell(position);
		var distant = new Vector2(512f, 512f);
		var distantCell = cognition.Cognition.GetCell(distant);

		return new WorldCognitionIntegrationResult(
			PresenceAfterActivity: cellAfterActivity.Presence,
			DisturbanceAfterActivity: cellAfterActivity.Disturbance,
			BearAffinityAfterActivity: cellAfterActivity.BearAffinity,
			RavenAffinityAfterActivity: cellAfterActivity.RavenAffinity,
			AwarenessAfterActivity: awareness,
			RegionalMoodAfterActivity: mood,
			DominantInterest: interest.DominantInterest,
			PresenceAfterDecay: cellAfterDecay.Presence,
			DisturbanceAfterDecay: cellAfterDecay.Disturbance,
			DistantPresence: distantCell.Presence,
			DistantDisturbance: distantCell.Disturbance);
	}
}

public sealed record WorldCognitionIntegrationResult(
	float PresenceAfterActivity,
	float DisturbanceAfterActivity,
	float BearAffinityAfterActivity,
	float RavenAffinityAfterActivity,
	AwarenessState AwarenessAfterActivity,
	RegionalMood RegionalMoodAfterActivity,
	AffinityType DominantInterest,
	float PresenceAfterDecay,
	float DisturbanceAfterDecay,
	float DistantPresence,
	float DistantDisturbance);
