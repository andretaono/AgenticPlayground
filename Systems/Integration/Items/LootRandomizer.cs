using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Integration.Items;

public sealed class LootRandomizer
{
	private readonly IItemFactory _itemFactory;
	private readonly ModifierCatalog _catalog;
	private readonly Random _random;

	public LootRandomizer(IItemFactory itemFactory, ModifierCatalog catalog, Random? random = null)
	{
		_itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
		_catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
		_random = random ?? Random.Shared;
	}

	public Item Roll(int minModifiers, int maxModifiers)
	{
		if (minModifiers < 0)
			throw new ArgumentOutOfRangeException(nameof(minModifiers), "Minimum modifier count must be non-negative.");

		if (maxModifiers < minModifiers)
			throw new ArgumentOutOfRangeException(nameof(maxModifiers), "Maximum modifier count must be greater than or equal to minimum.");

		var pool = _catalog.All;
		if (pool.Count == 0)
			return _itemFactory.Create(Array.Empty<Modifier>());

		var cappedMax = Math.Min(maxModifiers, pool.Count);
		var cappedMin = Math.Min(minModifiers, cappedMax);
		var count = _random.Next(cappedMin, cappedMax + 1);
		var modifiers = PickModifiers(pool, count);

		return _itemFactory.Create(modifiers);
	}

	private List<Modifier> PickModifiers(IReadOnlyList<CatalogModifier> pool, int count)
	{
		var remaining = pool.ToList();
		var picked = new List<Modifier>(count);

		for (var i = 0; i < count && remaining.Count > 0; i++)
		{
			var selectedIndex = PickWeightedIndex(remaining);
			picked.Add(remaining[selectedIndex].ToModifier());
			remaining.RemoveAt(selectedIndex);
		}

		return picked;
	}

	private int PickWeightedIndex(IReadOnlyList<CatalogModifier> pool)
	{
		var totalWeight = 0f;

		foreach (var entry in pool)
		{
			if (entry.Weight <= 0f)
				throw new InvalidOperationException($"Modifier '{entry.Id}' has invalid weight {entry.Weight}.");

			totalWeight += entry.Weight;
		}

		var roll = (float)(_random.NextDouble() * totalWeight);
		var cumulative = 0f;

		for (var i = 0; i < pool.Count; i++)
		{
			cumulative += pool[i].Weight;
			if (roll < cumulative)
				return i;
		}

		return pool.Count - 1;
	}
}
