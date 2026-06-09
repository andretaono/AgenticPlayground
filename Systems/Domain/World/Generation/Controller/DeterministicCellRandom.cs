namespace Game.Systems.Domain.World.Generation.Controller;

internal static class DeterministicCellRandom
{
	public static float Roll(int seed, int x, int y, int salt = 0)
	{
		unchecked
		{
			var hash = seed;
			hash = (hash * 31) + x;
			hash = (hash * 31) + y;
			hash = (hash * 31) + salt;
			hash ^= hash << 13;
			hash ^= hash >> 17;
			hash ^= hash << 5;
			return (hash & 0x7FFFFFFF) / (float)int.MaxValue;
		}
	}
}
