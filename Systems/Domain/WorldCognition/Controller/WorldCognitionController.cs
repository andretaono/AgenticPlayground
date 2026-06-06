using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;

namespace Game.Systems.Domain.WorldCognition.Controller;

internal sealed class WorldCognitionController : IWorldCognitionController
{
	private readonly WorldCognitionGridStore _store;

	public WorldCognitionController(WorldCognitionGridStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void AddPresence(Vector2 position, float amount)
	{
		ValidateAmount(amount);
		var (x, y) = ResolveCell(position);
		_store.AddPresence(x, y, amount);
	}

	public void AddDisturbance(Vector2 position, float amount)
	{
		ValidateAmount(amount);
		var (x, y) = ResolveCell(position);
		_store.AddDisturbance(x, y, amount);
	}

	public void AddAffinity(Vector2 position, AffinityType affinityType, float amount)
	{
		ValidateAmount(amount);
		var (x, y) = ResolveCell(position);
		_store.AddAffinity(x, y, affinityType, amount);
	}

	public WorldCell GetCell(Vector2 position)
	{
		var (x, y) = ResolveCell(position);
		return _store.GetCell(x, y);
	}

	public AwarenessState GetAwareness(Vector2 position)
	{
		var (x, y) = ResolveCell(position);
		var region = _store.GetRegionalAverages(x, y, _store.Config.QueryRadiusCells);
		var awareness =
			region.AveragePresence * _store.Config.AwarenessPresenceWeight +
			region.AverageDisturbance * _store.Config.AwarenessDisturbanceWeight;

		return MapAwareness(Math.Clamp(awareness, 0f, 100f));
	}

	public RegionalMood GetRegionalMood(Vector2 position)
	{
		var (x, y) = ResolveCell(position);
		var region = _store.GetRegionalAverages(x, y, _store.Config.QueryRadiusCells);
		return MapRegionalMood(Math.Clamp(region.AverageDisturbance, 0f, 100f));
	}

	public EcologicalInterest GetEcologicalInterest(Vector2 position)
	{
		var (x, y) = ResolveCell(position);
		var region = _store.GetRegionalAverages(x, y, _store.Config.QueryRadiusCells);
		var total = region.TotalAffinity.Bear + region.TotalAffinity.Raven + region.TotalAffinity.Seal;

		if (total <= 0f)
			return new EcologicalInterest(0f, 0f, 0f, AffinityType.Bear);

		var bear = region.TotalAffinity.Bear / total * 100f;
		var raven = region.TotalAffinity.Raven / total * 100f;
		var seal = region.TotalAffinity.Seal / total * 100f;

		return new EcologicalInterest(bear, raven, seal, ResolveDominantInterest(bear, raven, seal));
	}

	private (int X, int Y) ResolveCell(Vector2 position)
	{
		if (_store.Config.CellSize <= 0f)
			throw new InvalidOperationException("CellSize must be greater than zero.");

		var x = (int)MathF.Floor(position.X / _store.Config.CellSize);
		var y = (int)MathF.Floor(position.Y / _store.Config.CellSize);

		if (x < 0 || y < 0 || x >= _store.Config.GridWidth || y >= _store.Config.GridHeight)
			throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the cognition grid.");

		return (x, y);
	}

	private static void ValidateAmount(float amount)
	{
		if (amount < 0f)
			throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
	}

	private static AwarenessState MapAwareness(float value) => value switch
	{
		< 20f => AwarenessState.Unnoticed,
		< 40f => AwarenessState.Noticed,
		< 60f => AwarenessState.Observed,
		< 80f => AwarenessState.Tracked,
		_ => AwarenessState.Hunted
	};

	private static RegionalMood MapRegionalMood(float value) => value switch
	{
		< 20f => RegionalMood.Quiet,
		< 40f => RegionalMood.Restless,
		< 60f => RegionalMood.Disturbed,
		< 80f => RegionalMood.Hostile,
		_ => RegionalMood.Violent
	};

	private static AffinityType ResolveDominantInterest(float bear, float raven, float seal)
	{
		if (raven >= bear && raven >= seal)
			return AffinityType.Raven;

		if (seal >= bear && seal >= raven)
			return AffinityType.Seal;

		return AffinityType.Bear;
	}
}
