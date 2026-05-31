using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Ports;

public interface IEntityResourceRegistry
{
	void AddResource(EntityId entityId, IResourceDefinition definition);
	bool RemoveResource(EntityId entityId, ResourceId resourceId);
	bool HasResource(EntityId entityId, ResourceId resourceId);
	T? TryGetDefinition<T>(EntityId entityId) where T : class, IResourceDefinition;
}
