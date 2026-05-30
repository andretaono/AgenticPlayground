namespace Game.Systems.Domain.EntityResource.Model;

internal sealed class EntityResourceState
{
	public ResourceId ResourceId = default!;
	public string Name = string.Empty;
	public float CurrentAmount;
	public float MaximumAmount;
	public float RegenerationRate;
	public float DepletionRate;
}
