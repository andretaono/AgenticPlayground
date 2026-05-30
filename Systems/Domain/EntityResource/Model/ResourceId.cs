namespace Game.Systems.Domain.EntityResource.Model;

public readonly record struct ResourceId(string Id)
{
	public override string ToString() => Id;
}
