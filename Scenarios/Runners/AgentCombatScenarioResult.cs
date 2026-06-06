namespace Game.Scenarios.Runners;

public sealed record AgentCombatScenarioResult(
	bool TargetDamaged,
	float InitialDistance,
	float FinalDistance,
	float FinalTargetHealth,
	float InitialTargetHealth);
