namespace Game.Systems.Domain.Navigation.Model;

public sealed class NavigationGrid
{
	private readonly int[,] _moveCosts;

	public NavigationGrid(int width, int height, int[,] moveCosts)
	{
		if (width <= 0)
			throw new ArgumentOutOfRangeException(nameof(width));
		if (height <= 0)
			throw new ArgumentOutOfRangeException(nameof(height));
		if (moveCosts is null)
			throw new ArgumentNullException(nameof(moveCosts));
		if (moveCosts.GetLength(0) != width || moveCosts.GetLength(1) != height)
			throw new ArgumentException("Move cost dimensions must match width and height.", nameof(moveCosts));

		Width = width;
		Height = height;
		_moveCosts = moveCosts;
	}

	public int Width { get; }
	public int Height { get; }

	public bool IsInBounds(int x, int y) =>
		x >= 0 && y >= 0 && x < Width && y < Height;

	public bool IsWalkable(int x, int y)
	{
		if (!IsInBounds(x, y))
			return false;

		return _moveCosts[x, y] > 0;
	}

	public int GetMoveCost(int x, int y)
	{
		if (!IsWalkable(x, y))
			throw new InvalidOperationException($"Tile ({x},{y}) is not walkable.");

		return _moveCosts[x, y];
	}
}
