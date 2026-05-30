using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Model;

internal sealed class EntityResourceStateStore
{
	private readonly Dictionary<EntityId, Dictionary<ResourceId, EntityResourceState>> _resources = new();

	public IReadOnlyDictionary<EntityId, Dictionary<ResourceId, EntityResourceState>> Resources => _resources;

	public EntityResourceState Get(EntityId entityId, ResourceId resourceId)
	{
		if (!_resources.TryGetValue(entityId, out var entityResources))
			throw new KeyNotFoundException($"No resources registered for entity '{entityId}'.");

		if (!entityResources.TryGetValue(resourceId, out var resource))
			throw new KeyNotFoundException($"Entity '{entityId}' has no resource '{resourceId}'.");

		return resource;
	}

	public bool HasResource(EntityId entityId, ResourceId resourceId) =>
		_resources.TryGetValue(entityId, out var entityResources) &&
		entityResources.ContainsKey(resourceId);

	public Dictionary<ResourceId, EntityResourceState> GetOrCreateEntityResources(EntityId entityId)
	{
		if (!_resources.TryGetValue(entityId, out var entityResources))
		{
			entityResources = new Dictionary<ResourceId, EntityResourceState>();
			_resources[entityId] = entityResources;
		}

		return entityResources;
	}

	public bool RemoveResource(EntityId entityId, ResourceId resourceId) =>
		_resources.TryGetValue(entityId, out var entityResources) &&
		entityResources.Remove(resourceId);
}
