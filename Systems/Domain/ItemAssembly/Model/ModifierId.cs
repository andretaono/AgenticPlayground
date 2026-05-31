namespace Game.Systems.Domain.ItemAssembly.Model;

public readonly record struct ModifierId(string Value)
{
	public override string ToString() => Value;
}
