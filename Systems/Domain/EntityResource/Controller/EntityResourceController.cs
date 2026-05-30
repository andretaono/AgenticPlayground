using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Controller;

internal sealed class EntityResourceController : IEntityResourceController
{
	private readonly EntityResourceStateStore _store;

	public EntityResourceController(EntityResourceStateStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public void IncreaseResource(EntityId entityId, ResourceId resourceId, float amount)
	{
		if (amount < 0f)
			throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

		var resource = _store.Get(entityId, resourceId);
		resource.CurrentAmount = Math.Min(resource.CurrentAmount + amount, resource.MaximumAmount);
	}

	public void DecreaseResource(EntityId entityId, ResourceId resourceId, float amount)
	{
		if (amount < 0f)
			throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");

		var resource = _store.Get(entityId, resourceId);
		resource.CurrentAmount = Math.Max(resource.CurrentAmount - amount, 0f);
	}

	public void SetResource(EntityId entityId, ResourceId resourceId, float amount)
	{
		var resource = _store.Get(entityId, resourceId);
		resource.CurrentAmount = Math.Clamp(amount, 0f, resource.MaximumAmount);
	}

	public ResourceSnapshot GetResource(EntityId entityId, ResourceId resourceId)
	{
		var resource = _store.Get(entityId, resourceId);
		return ToSnapshot(resource);
	}

	public bool IsDepleted(EntityId entityId, ResourceId resourceId) =>
		_store.Get(entityId, resourceId).CurrentAmount <= 0f;

	public bool IsFull(EntityId entityId, ResourceId resourceId)
	{
		var resource = _store.Get(entityId, resourceId);
		return resource.CurrentAmount >= resource.MaximumAmount;
	}

	private static ResourceSnapshot ToSnapshot(EntityResourceState resource) =>
		new(
			resource.ResourceId,
			resource.Name,
			resource.CurrentAmount,
			resource.MaximumAmount,
			resource.RegenerationRate,
			resource.DepletionRate);
}
