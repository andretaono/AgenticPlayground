namespace Game.Systems.Domain.AgentMovement.Model;

public sealed record AgentMovementConfig(
    float GroundSpeed,
    float SwimSpeed,
    float AirSpeed
)
{
    public static readonly AgentMovementConfig Default = new(
        GroundSpeed: 5f,
        SwimSpeed: 3f,
        AirSpeed: 4f
    );
}

