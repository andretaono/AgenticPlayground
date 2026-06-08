using Game.Systems.Domain.World.Generation.Model;

namespace Game.Systems.Domain.World.Generation.Ports;

public interface IWorldGenerator
{
	GeneratedWorldMap Generate(WorldGenerationConfig config);
}
