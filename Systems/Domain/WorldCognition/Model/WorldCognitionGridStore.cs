namespace Game.Systems.Domain.WorldCognition.Model;

internal sealed class WorldCognitionGridStore
{
	private readonly float[,] _presence;
	private readonly float[,] _disturbance;
	private readonly AffinityCell[,] _affinity;

	public WorldCognitionGridStore(WorldCognitionConfig config)
	{
		if (config.GridWidth <= 0)
			throw new ArgumentOutOfRangeException(nameof(config), "GridWidth must be greater than zero.");
		if (config.GridHeight <= 0)
			throw new ArgumentOutOfRangeException(nameof(config), "GridHeight must be greater than zero.");

		Config = config;
		_presence = new float[config.GridWidth, config.GridHeight];
		_disturbance = new float[config.GridWidth, config.GridHeight];
		_affinity = new AffinityCell[config.GridWidth, config.GridHeight];
	}

	public WorldCognitionConfig Config { get; }

	public void AddPresence(int x, int y, float amount) =>
		_presence[x, y] = Math.Clamp(_presence[x, y] + amount, 0f, 100f);

	public void AddDisturbance(int x, int y, float amount) =>
		_disturbance[x, y] = Math.Clamp(_disturbance[x, y] + amount, 0f, 100f);

	public void AddAffinity(int x, int y, AffinityType affinityType, float amount)
	{
		var cell = _affinity[x, y].Add(affinityType, amount);
		_affinity[x, y] = cell.Clamp(0f, 100f);
	}

	public WorldCell GetCell(int x, int y)
	{
		var affinity = _affinity[x, y];
		return new WorldCell(
			_presence[x, y],
			_disturbance[x, y],
			affinity.Bear,
			affinity.Raven,
			affinity.Seal);
	}

	public void ApplyDecay(float deltaTime)
	{
		var presenceFactor = MathF.Pow(Config.PresenceDecayPerSecond, deltaTime);
		var disturbanceFactor = MathF.Pow(Config.DisturbanceDecayPerSecond, deltaTime);
		var affinityFactor = MathF.Pow(Config.AffinityDecayPerSecond, deltaTime);

		for (var x = 0; x < Config.GridWidth; x++)
		{
			for (var y = 0; y < Config.GridHeight; y++)
			{
				_presence[x, y] *= presenceFactor;
				_disturbance[x, y] *= disturbanceFactor;

				var affinity = _affinity[x, y];
				_affinity[x, y] = new AffinityCell(
					affinity.Bear * affinityFactor,
					affinity.Raven * affinityFactor,
					affinity.Seal * affinityFactor);
			}
		}
	}

	public (float AveragePresence, float AverageDisturbance, AffinityCell TotalAffinity) GetRegionalAverages(
		int centerX,
		int centerY,
		int radius)
	{
		var minX = Math.Max(0, centerX - radius);
		var maxX = Math.Min(Config.GridWidth - 1, centerX + radius);
		var minY = Math.Max(0, centerY - radius);
		var maxY = Math.Min(Config.GridHeight - 1, centerY + radius);

		var presenceSum = 0f;
		var disturbanceSum = 0f;
		var bearSum = 0f;
		var ravenSum = 0f;
		var sealSum = 0f;
		var count = 0;

		for (var x = minX; x <= maxX; x++)
		{
			for (var y = minY; y <= maxY; y++)
			{
				presenceSum += _presence[x, y];
				disturbanceSum += _disturbance[x, y];

				var affinity = _affinity[x, y];
				bearSum += affinity.Bear;
				ravenSum += affinity.Raven;
				sealSum += affinity.Seal;
				count++;
			}
		}

		if (count == 0)
			return (0f, 0f, AffinityCell.Zero);

		return (
			presenceSum / count,
			disturbanceSum / count,
			new AffinityCell(bearSum, ravenSum, sealSum));
	}
}
