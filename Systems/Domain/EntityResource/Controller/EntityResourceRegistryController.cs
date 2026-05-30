using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Controller;

internal sealed class EntityResourceRegistryController : IEntityResourceRegistry
{
	private readonly EntityResourceStateStore _store;

	public EntityResourceRegistryController(EntityResourceStateStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void AddResource(EntityId entityId, ResourceDefinition definition)
	{
		if (definition is null) throw new ArgumentNullException(nameof(definition));
		if (definition.MaximumAmount <= 0f)
			throw new ArgumentOutOfRangeException(nameof(definition), "MaximumAmount must be greater than zero.");
		if (definition.InitialAmount < 0f || definition.InitialAmount > definition.MaximumAmount)
			throw new ArgumentOutOfRangeException(nameof(definition), "InitialAmount must be between 0 and MaximumAmount.");

		var entityResources = _store.GetOrCreateEntityResources(entityId);
		if (entityResources.ContainsKey(definition.ResourceId))
			throw new InvalidOperationException($"Entity '{entityId}' already has resource '{definition.ResourceId}'.");

		entityResources[definition.ResourceId] = new EntityResourceState
		{
			ResourceId = definition.ResourceId,
			Name = definition.Name,
			CurrentAmount = definition.InitialAmount,
			MaximumAmount = definition.MaximumAmount,
			RegenerationRate = definition.RegenerationRate,
			DepletionRate = definition.DepletionRate
		};
	}

	public bool RemoveResource(EntityId entityId, ResourceId resourceId) =>
		_store.RemoveResource(entityId, resourceId);

	public bool HasResource(EntityId entityId, ResourceId resourceId) =>
		_store.HasResource(entityId, resourceId);
}
