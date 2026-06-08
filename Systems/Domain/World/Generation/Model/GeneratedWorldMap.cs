using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;

namespace Game.Systems.Domain.World.Generation.Model;

public sealed class GeneratedWorldMap
{
	public GeneratedWorldMap(TileId[,] tiles, WorldPosition start, WorldPosition goal, int seedUsed)
	{
		Tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
		Start = start;
		Goal = goal;
		SeedUsed = seedUsed;
		Width = tiles.GetLength(0);
		Height = tiles.GetLength(1);
	}

	public TileId[,] Tiles { get; }
	public WorldPosition Start { get; }
	public WorldPosition Goal { get; }
	public int SeedUsed { get; }
	public int Width { get; }
	public int Height { get; }

	public IWorldDataSource ToDataSource() => new InMemoryWorldDataSource(Tiles);
}
