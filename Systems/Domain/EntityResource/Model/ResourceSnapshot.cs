namespace Game.Systems.Domain.EntityResource.Model;

public readonly record struct ResourceSnapshot(
	ResourceId ResourceId,
	string Name,
	float CurrentAmount,
	float MaximumAmount,
	float RegenerationRate,
	float DepletionRate);
