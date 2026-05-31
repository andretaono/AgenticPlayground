namespace Game.Systems.Domain.ItemAssembly.Model;

public sealed class ResolvedModifierSet
{
	internal ResolvedModifierSet(
		IReadOnlyDictionary<ModifierId, float> flatValues,
		IReadOnlyDictionary<ModifierId, float> percentValues,
		IReadOnlySet<ModifierId> flags,
		IReadOnlyList<Modifier> rawModifiers)
	{
		FlatValues = flatValues;
		PercentValues = percentValues;
		Flags = flags;
		RawModifiers = rawModifiers;
	}

	public IReadOnlyDictionary<ModifierId, float> FlatValues { get; }

	public IReadOnlyDictionary<ModifierId, float> PercentValues { get; }

	public IReadOnlySet<ModifierId> Flags { get; }

	public IReadOnlyList<Modifier> RawModifiers { get; }
}
