namespace Game.Tests.Integration.Runners;

public sealed record AgentCombatIntegrationResult(
	bool TargetDamaged,
	float InitialDistance,
	float FinalDistance,
	float FinalTargetHealth,
	float InitialTargetHealth);
