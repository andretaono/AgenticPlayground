namespace Game.Systems.Domain.EntityResource.Model;

public sealed record ResourceDefinition(
	ResourceId ResourceId,
	string Name,
	float MaximumAmount,
	float RegenerationRate,
	float DepletionRate,
	float InitialAmount);
