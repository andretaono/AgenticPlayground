namespace Game.Systems.Domain.AgentMovement.Model;

public sealed record AgentMovementConfig(
	float GroundSpeed,
	float SwimSpeed,
	float AirSpeed
);
