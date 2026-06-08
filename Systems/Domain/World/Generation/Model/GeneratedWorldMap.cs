using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;

namespace Game.Systems.Domain.World.Generation.Model;

public sealed class GeneratedWorldMap
{
	public GeneratedWorldMap(
		TileId[,] groundLayer,
		WorldPosition start,
		WorldPosition goal,
		int seedUsed)
	{
		GroundLayer = groundLayer ?? throw new ArgumentNullException(nameof(groundLayer));
		Start = start;
		Goal = goal;
		SeedUsed = seedUsed;
		Width = groundLayer.GetLength(0);
		Height = groundLayer.GetLength(1);
	}

	public TileId[,] GroundLayer { get; }

	/// <summary>Alias for <see cref="GroundLayer"/>.</summary>
	public TileId[,] Tiles => GroundLayer;

	public WorldPosition Start { get; }
	public WorldPosition Goal { get; }
	public int SeedUsed { get; }
	public int Width { get; }
	public int Height { get; }

	public IWorldDataSource ToDataSource() => new InMemoryWorldDataSource(GroundLayer);
}
