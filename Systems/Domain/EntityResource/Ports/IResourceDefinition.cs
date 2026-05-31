using Game.Systems.Domain.EntityResource.Model;

namespace Game.Systems.Domain.EntityResource.Ports;

public interface IResourceDefinition
{
	Type ResourceType { get; }
	ResourceId ResourceId { get; }
	string Name { get; }
	float MaximumAmount { get; }
	float RegenerationRate { get; }
	float DepletionRate { get; }
	float InitialAmount { get; }
	float CurrentAmount { get; }
	bool IsDepleted { get; }
	bool IsFull { get; }
	void Increase(float amount);
	void Decrease(float amount);
	void Set(float amount);
	ResourceSnapshot GetSnapshot();
}
