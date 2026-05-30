using Game.Systems.Domain.AgentCombat.Model;
using Game.Systems.Domain.AgentCombat.Ports;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Integration.Combat;

public sealed class ResourceDamageEffect : IEffect
{
	private readonly IEntityResourceController _resources;
	private readonly ResourceId _resourceId;

	public ResourceDamageEffect(IEntityResourceController resources, ResourceId resourceId)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_resourceId = resourceId;
	}

	public void Apply(EffectContext context) =>
		_resources.DecreaseResource(context.Target.EntityId, _resourceId, context.Power);
}
