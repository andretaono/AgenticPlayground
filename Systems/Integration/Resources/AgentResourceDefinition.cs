using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Resources;

public abstract class AgentResourceDefinition : ResourceDefinitionBase
{
	protected AgentResourceDefinition(
		Type resourceType,
		EntityId owner,
		string key,
		string name,
		float maximumAmount,
		float regenerationRate,
		float depletionRate,
		float initialAmount)
		: base(
			resourceType,
			new ResourceId($"{key}-{owner.Value}"),
			name,
			maximumAmount,
			regenerationRate,
			depletionRate,
			initialAmount)
	{
	}

	public void Attach(IEntityResourceRegistry registry, EntityId owner) =>
		registry.AddResource(owner, this);
}
