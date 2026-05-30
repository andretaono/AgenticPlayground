using System.Text;
using Game.Systems.Domain.World.Ports;
using Game.Systems.Domain.World.Model;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Renders a top-down world map to the console with a player marker.
/// </summary>
public sealed class ConsoleWorldRenderer
{
	private readonly DefaultTileVisualMapper _visual;

	public ConsoleWorldRenderer(DefaultTileVisualMapper visual)
	{
		_visual = visual ?? throw new ArgumentNullException(nameof(visual));
	}

	public void Render(IWorldSystem world, int width, int height, int playerTileX, int playerTileY)
	{
		var output = new StringBuilder();

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (x == playerTileX && y == playerTileY)
					output.Append('@');
				else
				{
					var tileId = world.GetTileId(new WorldPosition(x, y));
					output.Append(_visual.MapToChar(tileId));
				}
			}

			output.AppendLine();
		}

		Console.Write(output);
	}
}
