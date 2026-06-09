namespace Game.Systems.Integration.Adapters;

internal static class MovementFootprint
{
	public static bool CircleFits(
		float centerX,
		float centerY,
		float radius,
		int tileSize,
		Func<int, int, bool> isBlocked)
	{
		if (radius <= 0f)
			return !isBlocked(
				(int)MathF.Floor(centerX / tileSize),
				(int)MathF.Floor(centerY / tileSize));

		var minTileX = (int)MathF.Floor((centerX - radius) / tileSize);
		var maxTileX = (int)MathF.Floor((centerX + radius) / tileSize);
		var minTileY = (int)MathF.Floor((centerY - radius) / tileSize);
		var maxTileY = (int)MathF.Floor((centerY + radius) / tileSize);

		for (var tileY = minTileY; tileY <= maxTileY; tileY++)
		{
			for (var tileX = minTileX; tileX <= maxTileX; tileX++)
			{
				if (!isBlocked(tileX, tileY))
					continue;

				if (CircleIntersectsTile(centerX, centerY, radius, tileX, tileY, tileSize))
					return false;
			}
		}

		return true;
	}

	private static bool CircleIntersectsTile(
		float centerX,
		float centerY,
		float radius,
		int tileX,
		int tileY,
		int tileSize)
	{
		var tileMinX = tileX * tileSize;
		var tileMinY = tileY * tileSize;
		var tileMaxX = tileMinX + tileSize;
		var tileMaxY = tileMinY + tileSize;

		var closestX = Math.Clamp(centerX, tileMinX, tileMaxX);
		var closestY = Math.Clamp(centerY, tileMinY, tileMaxY);
		var dx = centerX - closestX;
		var dy = centerY - closestY;
		return dx * dx + dy * dy <= radius * radius;
	}
}
