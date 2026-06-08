using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.TerrainMesh.Ports;

namespace Game.Systems.Domain.TerrainMesh.Controller;

internal sealed class HeightmapSamplerController : IHeightmapSampler
{
	public float Sample(Heightmap heightmap, int x, int y)
	{
		if (heightmap is null)
			throw new ArgumentNullException(nameof(heightmap));

		return heightmap.Sample(x, y);
	}

	public float SampleBilinear(Heightmap heightmap, float worldX, float worldZ)
	{
		if (heightmap is null)
			throw new ArgumentNullException(nameof(heightmap));

		var gridX = worldX / heightmap.CellSize;
		var gridZ = worldZ / heightmap.CellSize;

		var x0 = (int)MathF.Floor(gridX);
		var z0 = (int)MathF.Floor(gridZ);
		var tx = gridX - x0;
		var tz = gridZ - z0;

		var x1 = Math.Min(x0 + 1, heightmap.Width - 1);
		var z1 = Math.Min(z0 + 1, heightmap.Height - 1);
		x0 = Math.Max(x0, 0);
		z0 = Math.Max(z0, 0);

		var h00 = heightmap.SampleUnchecked(x0, z0);
		var h10 = heightmap.SampleUnchecked(x1, z0);
		var h01 = heightmap.SampleUnchecked(x0, z1);
		var h11 = heightmap.SampleUnchecked(x1, z1);

		var alongX0 = Lerp(h00, h10, tx);
		var alongX1 = Lerp(h01, h11, tx);
		return Lerp(alongX0, alongX1, tz);
	}

	private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
