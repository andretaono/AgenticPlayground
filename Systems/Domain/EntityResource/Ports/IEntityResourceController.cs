using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Ports;

public interface IEntityResourceController
{
	void IncreaseResource(EntityId entityId, ResourceId resourceId, float amount);
	void DecreaseResource(EntityId entityId, ResourceId resourceId, float amount);
	void SetResource(EntityId entityId, ResourceId resourceId, float amount);

	ResourceSnapshot GetResource(EntityId entityId, ResourceId resourceId);
	bool IsDepleted(EntityId entityId, ResourceId resourceId);
	bool IsFull(EntityId entityId, ResourceId resourceId);
}
