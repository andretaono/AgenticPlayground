using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Domain.World.Generation.Ports;

namespace Game.Systems.Domain.World.Generation.Controller;

internal sealed class WorldGeneratorController : IWorldGenerator
{
	public GeneratedWorldMap Generate(WorldGenerationConfig config)
	{
		if (config is null)
			throw new ArgumentNullException(nameof(config));
		if (config.Width < 3)
			throw new ArgumentOutOfRangeException(nameof(config), "Width must be at least 3.");
		if (config.Height < 3)
			throw new ArgumentOutOfRangeException(nameof(config), "Height must be at least 3.");
		if (config.FillProbability < 0f || config.FillProbability > 1f)
			throw new ArgumentOutOfRangeException(nameof(config), "FillProbability must be between 0 and 1.");
		if (config.CellularAutomataIterations < 0)
			throw new ArgumentOutOfRangeException(nameof(config), "CellularAutomataIterations must be non-negative.");
		if (config.MaxAttempts < 1)
			throw new ArgumentOutOfRangeException(nameof(config), "MaxAttempts must be at least 1.");
		if (config.WaterPoolAttempts < 0)
			throw new ArgumentOutOfRangeException(nameof(config), "WaterPoolAttempts must be non-negative.");
		if (config.WaterPoolMaxSize < 0)
			throw new ArgumentOutOfRangeException(nameof(config), "WaterPoolMaxSize must be non-negative.");

		for (var attempt = 0; attempt < config.MaxAttempts; attempt++)
		{
			var seed = config.Seed + attempt;
			var tiles = CaveCellularAutomata.Generate(
				seed,
				config.Width,
				config.Height,
				config.FillProbability,
				config.CellularAutomataIterations);

			if (!GroundConnectivity.TryPickStartAndGoal(tiles, out var start, out var goal))
				continue;

			if (!GroundConnectivity.HasGroundPath(tiles, start, goal))
				continue;

			WaterPlacer.Apply(tiles, start, goal, seed, config);

			if (!GroundConnectivity.HasGroundPath(tiles, start, goal))
				continue;

			return new GeneratedWorldMap(tiles, start, goal, seed);
		}

		throw new InvalidOperationException(
			$"Failed to generate a playable world after {config.MaxAttempts} attempt(s).");
	}
}
