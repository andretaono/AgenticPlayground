namespace Game.Systems.Domain.ItemAssembly.Model;

public readonly record struct ItemId(int Value)
{
	public override string ToString() => Value.ToString();
}
