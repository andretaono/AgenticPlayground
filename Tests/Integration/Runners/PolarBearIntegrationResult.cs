namespace Game.Tests.Integration.Runners;

public sealed record PolarBearIntegrationResult(
	bool AttackCommitted,
	int FirstAttackTick,
	float FinalPlayerHealth,
	float InitialPlayerHealth,
	bool TrackingDetected,
	bool AdvantageWithoutLowHealth,
	IReadOnlyList<string> BehaviourTrace);
