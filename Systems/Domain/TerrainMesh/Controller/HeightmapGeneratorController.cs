using Game.Systems.Domain.TerrainMesh.Model;
using Game.Systems.Domain.TerrainMesh.Ports;
using Game.Systems.Foundation.Noise;

namespace Game.Systems.Domain.TerrainMesh.Controller;

internal sealed class HeightmapGeneratorController : IHeightmapGenerator
{
	public Heightmap Generate(int seed, int width, int height, TerrainMeshConfig config)
	{
		if (width < 1)
			throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
		if (height < 1)
			throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 1.");
		if (config is null)
			throw new ArgumentNullException(nameof(config));
		if (config.CellSize <= 0f)
			throw new ArgumentOutOfRangeException(nameof(config), "CellSize must be greater than zero.");
		if (config.MaxHeight < config.MinHeight)
			throw new ArgumentOutOfRangeException(nameof(config), "MaxHeight must be greater than or equal to MinHeight.");

		var samples = new float[width, height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var noise = ValueNoise2D.Fractal(
					seed,
					x * config.NoiseFrequency,
					y * config.NoiseFrequency,
					config.NoiseOctaves,
					config.NoisePersistence,
					config.NoiseLacunarity);

				samples[x, y] = config.MinHeight + noise * (config.MaxHeight - config.MinHeight);
			}
		}

		return new Heightmap(samples, config.CellSize);
	}
}
