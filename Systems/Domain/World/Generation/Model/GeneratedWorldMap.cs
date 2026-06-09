using Game.Systems.Domain.World.Model;

using Game.Systems.Domain.World.Ports;



namespace Game.Systems.Domain.World.Generation.Model;



public sealed class GeneratedWorldMap

{

	public GeneratedWorldMap(

		TileId[,] groundLayer,

		WorldPosition start,

		WorldPosition goal,

		int seedUsed,

		TileId[,]? ceilingLayer = null,

		int[,]? caveRegionIndex = null,

		CaveCarveDiagnostic? caveCarveDiagnostic = null)

	{

		GroundLayer = groundLayer ?? throw new ArgumentNullException(nameof(groundLayer));

		Start = start;

		Goal = goal;

		SeedUsed = seedUsed;

		Width = groundLayer.GetLength(0);

		Height = groundLayer.GetLength(1);

		CeilingLayer = ceilingLayer ?? CreateAirCeilingLayer(Width, Height);

		CaveRegionIndex = caveRegionIndex ?? CreateEmptyRegionIndex(Width, Height);

		CaveCarveDiagnostic = caveCarveDiagnostic ?? CaveCarveDiagnostic.Empty;

	}



	public TileId[,] GroundLayer { get; }



	/// <summary>Alias for <see cref="GroundLayer"/>.</summary>

	public TileId[,] Tiles => GroundLayer;



	public TileId[,] CeilingLayer { get; }



	/// <summary>Assigned cave region id per cell, or -1 when not part of a roofed cave.</summary>

	public int[,] CaveRegionIndex { get; }

	public CaveCarveDiagnostic CaveCarveDiagnostic { get; }

	public WorldPosition Start { get; }

	public WorldPosition Goal { get; }

	public int SeedUsed { get; }

	public int Width { get; }

	public int Height { get; }



	public CoverKind CoverAt(int x, int y)

	{

		if (x < 0 || y < 0 || x >= Width || y >= Height)

			return CoverKind.OpenSky;



		var groundTile = GroundLayer[x, y];

		if (groundTile != TileIds.Ground && groundTile != TileIds.Water)

			return CoverKind.OpenSky;



		return CeilingLayer[x, y] == CeilingLayerTileIds.Solid

			? CoverKind.RoofedInterior

			: CoverKind.OpenSky;

	}



	public IWorldDataSource ToDataSource() => new InMemoryWorldDataSource(GroundLayer);



	private static TileId[,] CreateAirCeilingLayer(int width, int height)

	{

		var layer = new TileId[width, height];



		for (var y = 0; y < height; y++)

		for (var x = 0; x < width; x++)

			layer[x, y] = CeilingLayerTileIds.Air;



		return layer;

	}



	private static int[,] CreateEmptyRegionIndex(int width, int height)

	{

		var index = new int[width, height];



		for (var y = 0; y < height; y++)

		for (var x = 0; x < width; x++)

			index[x, y] = -1;



		return index;

	}

}


