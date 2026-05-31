namespace Game.Systems.Domain.ItemAssembly.Model;

public readonly record struct Modifier(
	ModifierId Id,
	ModifierKind Kind,
	float Value = 0f,
	int Priority = 0);
