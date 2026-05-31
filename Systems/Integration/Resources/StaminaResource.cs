using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Resources;

public sealed class StaminaResource : AgentResourceDefinition, IStaminaResourceDefinition
{
	public const string Key = "stamina";

	public StaminaResource(EntityId owner, float maximum = 50f, float depletionRate = 10f)
		: base(
			typeof(IStaminaResourceDefinition),
			owner,
			Key,
			"Stamina",
			maximum,
			regenerationRate: 0f,
			depletionRate,
			initialAmount: maximum)
	{
	}
}
