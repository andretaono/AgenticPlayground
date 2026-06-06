namespace Game.Scenarios.Runners;

public sealed record PolarBearScenarioResult(
	bool AttackCommitted,
	int FirstAttackTick,
	float FinalPlayerHealth,
	float InitialPlayerHealth,
	bool TrackingDetected,
	bool AdvantageWithoutLowHealth,
	IReadOnlyList<string> BehaviourTrace);
