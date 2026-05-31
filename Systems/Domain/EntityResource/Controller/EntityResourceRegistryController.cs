using Game.Systems.Domain.EntityResource.Model;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;

namespace Game.Systems.Domain.EntityResource.Controller;

internal sealed class EntityResourceRegistryController : IEntityResourceRegistry
{
	private readonly Dictionary<EntityId, Dictionary<Type, IResourceDefinition>> _definitionsByType = new();

	internal IEnumerable<IResourceDefinition> AllDefinitions =>
		_definitionsByType.Values.SelectMany(definitions => definitions.Values);

	public void AddResource(EntityId entityId, IResourceDefinition definition)
	{
		if (definition is null) throw new ArgumentNullException(nameof(definition));
		if (definition is not ResourceDefinitionBase registrable)
			throw new ArgumentException("Definition must extend ResourceDefinitionBase.", nameof(definition));
		if (definition.ResourceType is null)
			throw new ArgumentException("ResourceType must be specified.", nameof(definition));
		if (definition.MaximumAmount <= 0f)
			throw new ArgumentOutOfRangeException(nameof(definition), "MaximumAmount must be greater than zero.");
		if (definition.InitialAmount < 0f || definition.InitialAmount > definition.MaximumAmount)
			throw new ArgumentOutOfRangeException(nameof(definition), "InitialAmount must be between 0 and MaximumAmount.");

		var entityDefinitions = GetOrCreateEntityDefinitions(entityId);

		if (entityDefinitions.ContainsKey(definition.ResourceType))
			throw new InvalidOperationException(
				$"Entity '{entityId}' already has a resource of type '{definition.ResourceType.Name}'.");

		if (entityDefinitions.Values.Any(existing => existing.ResourceId == definition.ResourceId))
			throw new InvalidOperationException($"Entity '{entityId}' already has resource '{definition.ResourceId}'.");

		registrable.Register(entityId);
		entityDefinitions[definition.ResourceType] = definition;
	}

	public bool RemoveResource(EntityId entityId, ResourceId resourceId)
	{
		if (!_definitionsByType.TryGetValue(entityId, out var entityDefinitions))
			return false;

		foreach (var (resourceType, definition) in entityDefinitions.ToList())
		{
			if (definition.ResourceId != resourceId)
				continue;

			if (definition is ResourceDefinitionBase registrable)
				registrable.Unregister();

			entityDefinitions.Remove(resourceType);

			if (entityDefinitions.Count == 0)
				_definitionsByType.Remove(entityId);

			return true;
		}

		return false;
	}

	public bool HasResource(EntityId entityId, ResourceId resourceId) =>
		_definitionsByType.TryGetValue(entityId, out var entityDefinitions) &&
		entityDefinitions.Values.Any(definition => definition.ResourceId == resourceId);

	public T? TryGetDefinition<T>(EntityId entityId) where T : class, IResourceDefinition
	{
		if (!_definitionsByType.TryGetValue(entityId, out var entityDefinitions))
			return null;

		return entityDefinitions.TryGetValue(typeof(T), out var definition)
			? definition as T
			: null;
	}

	private Dictionary<Type, IResourceDefinition> GetOrCreateEntityDefinitions(EntityId entityId)
	{
		if (!_definitionsByType.TryGetValue(entityId, out var entityDefinitions))
		{
			entityDefinitions = new Dictionary<Type, IResourceDefinition>();
			_definitionsByType[entityId] = entityDefinitions;
		}

		return entityDefinitions;
	}
}
