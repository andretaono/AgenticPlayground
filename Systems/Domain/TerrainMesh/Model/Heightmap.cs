namespace Game.Systems.Domain.TerrainMesh.Model;

public sealed class Heightmap
{
	private readonly float[,] _samples;

	internal Heightmap(float[,] samples, float cellSize)
	{
		if (samples is null)
			throw new ArgumentNullException(nameof(samples));

		Width = samples.GetLength(0);
		Height = samples.GetLength(1);

		if (Width < 1 || Height < 1)
			throw new ArgumentException("Heightmap must contain at least one sample.", nameof(samples));

		if (cellSize <= 0f)
			throw new ArgumentOutOfRangeException(nameof(cellSize), "CellSize must be greater than zero.");

		_samples = samples;
		CellSize = cellSize;
	}

	public int Width { get; }
	public int Height { get; }
	public float CellSize { get; }

	public float Sample(int x, int y)
	{
		ValidateSampleIndex(x, y);
		return _samples[x, y];
	}

	public static Heightmap FromSamples(float[,] samples, float cellSize = 1f) =>
		new(samples, cellSize);

	internal float SampleUnchecked(int x, int y) => _samples[x, y];

	private void ValidateSampleIndex(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
			throw new ArgumentOutOfRangeException($"Sample index ({x}, {y}) is outside the heightmap.");
	}
}
