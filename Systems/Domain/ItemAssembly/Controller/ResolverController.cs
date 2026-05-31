using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Controller;

internal sealed class ResolverController : IModifierResolver
{
	public ResolvedModifierSet Resolve(Assembly assembly)
	{
		if (assembly is null) throw new ArgumentNullException(nameof(assembly));

		var rawModifiers = CollectModifiers(assembly);
		var flatValues = new Dictionary<ModifierId, float>();
		var percentValues = new Dictionary<ModifierId, float>();
		var flagWinners = new Dictionary<ModifierId, Modifier>();

		foreach (var modifier in rawModifiers)
		{
			switch (modifier.Kind)
			{
				case ModifierKind.Flat:
					flatValues[modifier.Id] = flatValues.GetValueOrDefault(modifier.Id) + modifier.Value;
					break;

				case ModifierKind.Percent:
					percentValues[modifier.Id] = percentValues.GetValueOrDefault(modifier.Id) + modifier.Value;
					break;

				case ModifierKind.Flag:
					ResolveFlagConflict(flagWinners, modifier);
					break;

				default:
					throw new InvalidOperationException($"Unsupported modifier kind '{modifier.Kind}'.");
			}
		}

		var flags = flagWinners.Values
			.Where(modifier => modifier.Value > 0f)
			.Select(modifier => modifier.Id)
			.ToHashSet();

		return new ResolvedModifierSet(flatValues, percentValues, flags, rawModifiers);
	}

	private static IReadOnlyList<Modifier> CollectModifiers(Assembly assembly)
	{
		var modifiers = new List<Modifier>();

		for (var socketIndex = 0; socketIndex < assembly.SocketCount; socketIndex++)
		{
			var item = assembly.GetItem(socketIndex);
			if (item is null)
				continue;

			modifiers.AddRange(item.Modifiers);
		}

		return modifiers
			.OrderByDescending(modifier => modifier.Priority)
			.ThenBy(modifier => modifier.Id.Value, StringComparer.Ordinal)
			.ThenBy(modifier => modifier.Kind)
			.ToList()
			.AsReadOnly();
	}

	private static void ResolveFlagConflict(Dictionary<ModifierId, Modifier> flagWinners, Modifier modifier)
	{
		if (!flagWinners.TryGetValue(modifier.Id, out var existing))
		{
			flagWinners[modifier.Id] = modifier;
			return;
		}

		if (modifier.Priority > existing.Priority)
			flagWinners[modifier.Id] = modifier;
	}
}
