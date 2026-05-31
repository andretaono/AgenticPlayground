using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Resources;

public sealed class ManaResource : AgentResourceDefinition, IManaResourceDefinition
{
	public const string Key = "mana";

	public ManaResource(EntityId owner, float maximum = 40f, float regenerationRate = 8f)
		: base(
			typeof(IManaResourceDefinition),
			owner,
			Key,
			"Mana",
			maximum,
			regenerationRate,
			depletionRate: 0f,
			initialAmount: 0f)
	{
	}
}
