using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Resources;

public sealed class HealthResource : AgentResourceDefinition, IHealthResourceDefinition
{
	public const string Key = "health";

	public HealthResource(EntityId owner, float maximum = 100f)
		: base(
			typeof(IHealthResourceDefinition),
			owner,
			Key,
			"Health",
			maximum,
			regenerationRate: 0f,
			depletionRate: 0f,
			initialAmount: maximum)
	{
	}
}
