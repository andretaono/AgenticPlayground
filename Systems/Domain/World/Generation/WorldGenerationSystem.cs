using Game.Systems.Domain.World.Generation.Controller;
using Game.Systems.Domain.World.Generation.Ports;

namespace Game.Systems.Domain.World.Generation;

/// <summary>
/// Root orchestrator for procedural world map generation.
/// </summary>
public sealed class WorldGenerationSystem
{
	public IWorldGenerator Generator { get; }

	public WorldGenerationSystem()
	{
		Generator = new WorldGeneratorController();
	}
}
