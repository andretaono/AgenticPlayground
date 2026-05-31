using Game.Systems.Domain.EntityResource.Ports;

namespace Game.Systems.Domain.EntityResource.Model;

public sealed class ResourceDefinition : ResourceDefinitionBase
{
	public ResourceDefinition(
		Type resourceType,
		ResourceId resourceId,
		string name,
		float maximumAmount,
		float regenerationRate,
		float depletionRate,
		float initialAmount)
		: base(
			resourceType,
			resourceId,
			name,
			maximumAmount,
			regenerationRate,
			depletionRate,
			initialAmount)
	{
	}
}
