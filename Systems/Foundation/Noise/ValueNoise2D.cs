namespace Game.Systems.Foundation.Noise;

/// <summary>
/// Deterministic 2D value noise with fractal layering. Engine-agnostic; seed + coordinates only.
/// </summary>
public static class ValueNoise2D
{
	public static float Sample(int seed, float x, float y)
	{
		var x0 = (int)MathF.Floor(x);
		var y0 = (int)MathF.Floor(y);
		var tx = Smooth(x - x0);
		var ty = Smooth(y - y0);

		var v00 = Hash(seed, x0, y0);
		var v10 = Hash(seed, x0 + 1, y0);
		var v01 = Hash(seed, x0, y0 + 1);
		var v11 = Hash(seed, x0 + 1, y0 + 1);

		var alongX0 = Lerp(v00, v10, tx);
		var alongX1 = Lerp(v01, v11, tx);
		return Lerp(alongX0, alongX1, ty);
	}

	public static float Fractal(
		int seed,
		float x,
		float y,
		int octaves,
		float persistence = 0.5f,
		float lacunarity = 2f)
	{
		if (octaves < 1)
			throw new ArgumentOutOfRangeException(nameof(octaves), "Octaves must be at least 1.");

		var amplitude = 1f;
		var frequency = 1f;
		var sum = 0f;
		var maxAmplitude = 0f;

		for (var octave = 0; octave < octaves; octave++)
		{
			sum += Sample(seed, x * frequency, y * frequency) * amplitude;
			maxAmplitude += amplitude;
			amplitude *= persistence;
			frequency *= lacunarity;
		}

		return maxAmplitude <= 0f ? 0f : sum / maxAmplitude;
	}

	private static float Hash(int seed, int x, int y)
	{
		unchecked
		{
			var hash = seed;
			hash = hash * 374761393 + x;
			hash = hash * 668265263 + y;
			hash = (hash ^ (hash >> 13)) * 1274126177;
			hash ^= hash >> 16;
			return (hash & 0x7fffffff) / (float)int.MaxValue;
		}
	}

	private static float Smooth(float t) => t * t * (3f - 2f * t);

	private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
