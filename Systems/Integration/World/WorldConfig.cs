using Game.Systems.Domain.World.Generation.Model;

namespace Game.Systems.Integration.World;

public sealed class WorldConfig
{
	public static WorldConfig Default { get; } = new();

	public WorldGenerationConfig Generation { get; init; } = new()
	{
		MaxCavesPerBlob = 3
	};
}
