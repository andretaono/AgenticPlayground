using Game.Systems.Domain.AgentMovement.Model;

namespace Game.Systems.Integration.Player;

public sealed class PlayerConfig
{
	public static PlayerConfig Default { get; } = new();

	public float GroundSpeed { get; init; } = 4f;
	public float SwimSpeed { get; init; } = 2.5f;
	public float BodyRadius { get; init; } = 0.4f;
	public float MaxHealth { get; init; } = 100f;
	public float TurnSpeedDegrees { get; init; } = 180f;
	public float CharacterHalfHeight { get; init; } = 0.5f;

	public AgentMovementConfig ToMovementConfig() =>
		new(GroundSpeed, SwimSpeed, GroundSpeed, BodyRadius);
}
