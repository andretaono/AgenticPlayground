using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.World.Model;
using Game.Systems.Integration.Adapters;

namespace Game.Systems.Integration.Adapters;

public sealed class WalkableSurfaceHeightSampler
{
	private readonly ITileRulesProvider _tileRules;
	private readonly TileId[,] _map;
	private readonly int _width;
	private readonly int _height;

	public WalkableSurfaceHeightSampler(
		ITileRulesProvider tileRules,
		InMemoryWorldDataSource worldData)
	{
		_tileRules = tileRules ?? throw new ArgumentNullException(nameof(tileRules));
		if (worldData is null)
			throw new ArgumentNullException(nameof(worldData));

		_map = worldData.LoadMap();
		_width = worldData.Width;
		_height = worldData.Height;
	}

	public float Sample(
		Heightmap heightmap,
		float tileX,
		float tileY,
		float heightScale)
	{
		if (heightmap is null)
			throw new ArgumentNullException(nameof(heightmap));

		var standTileX = (int)MathF.Floor(tileX);
		var standTileY = (int)MathF.Floor(tileY);
		var standHeight = SampleTileHeight(heightmap, standTileX, standTileY) * heightScale;

		var x0 = (int)MathF.Floor(tileX);
		var y0 = (int)MathF.Floor(tileY);
		var tx = tileX - x0;
		var ty = tileY - y0;
		var x1 = Math.Min(x0 + 1, _width - 1);
		var y1 = Math.Min(y0 + 1, _height - 1);
		x0 = Math.Max(x0, 0);
		y0 = Math.Max(y0, 0);

		var h00 = ResolveCornerHeight(heightmap, x0, y0, standTileX, standTileY, standHeight, heightScale);
		var h10 = ResolveCornerHeight(heightmap, x1, y0, standTileX, standTileY, standHeight, heightScale);
		var h01 = ResolveCornerHeight(heightmap, x0, y1, standTileX, standTileY, standHeight, heightScale);
		var h11 = ResolveCornerHeight(heightmap, x1, y1, standTileX, standTileY, standHeight, heightScale);

		var alongX0 = Lerp(h00, h10, tx);
		var alongX1 = Lerp(h01, h11, tx);
		return Lerp(alongX0, alongX1, ty);
	}

	private float ResolveCornerHeight(
		Heightmap heightmap,
		int cornerX,
		int cornerY,
		int standTileX,
		int standTileY,
		float standHeightScaled,
		float heightScale)
	{
		if (BlocksMovement(cornerX, cornerY))
			return standHeightScaled;

		return SampleTileHeight(heightmap, cornerX, cornerY) * heightScale;
	}

	private float SampleTileHeight(Heightmap heightmap, int tileX, int tileY)
	{
		if (tileX < 0 || tileY < 0 || tileX >= _width || tileY >= _height)
			return heightmap.Sample(
				Math.Clamp(tileX, 0, _width - 1),
				Math.Clamp(tileY, 0, _height - 1));

		return heightmap.Sample(tileX, tileY);
	}

	private bool BlocksMovement(int tileX, int tileY)
	{
		if (tileX < 0 || tileY < 0 || tileX >= _width || tileY >= _height)
			return true;

		return _tileRules.GetRules(_map[tileX, tileY]).HasFlag(TileRules.BlocksMovement);
	}

	private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
