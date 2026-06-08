using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Tests.Fakes;

public sealed class FakeHealthResource : ResourceDefinitionBase, IHealthResourceDefinition
{
	public FakeHealthResource(
		ResourceId resourceId,
		float maximum,
		float initialAmount = -1f,
		float regenerationRate = 0f)
		: base(
			typeof(IHealthResourceDefinition),
			resourceId,
			"health",
			maximum,
			regenerationRate,
			depletionRate: 0f,
			initialAmount < 0f ? maximum : initialAmount)
	{
	}

	public void Attach(IEntityResourceRegistry registry, EntityId entityId) =>
		registry.AddResource(entityId, this);
}
