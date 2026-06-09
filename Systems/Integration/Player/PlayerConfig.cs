using Game.Systems.Domain.AgentMovement.Model;

namespace Game.Systems.Integration.Player;

public sealed class PlayerConfig
{
	public float GroundSpeed { get; init; } = 4f;
	public float SwimSpeed { get; init; } = 2.5f;
	public float BodyRadius { get; init; } = 0.4f;
	public float MaxHealth { get; init; } = 100f;

	public AgentMovementConfig ToMovementConfig() =>
		new(GroundSpeed, SwimSpeed, GroundSpeed, BodyRadius);
}
