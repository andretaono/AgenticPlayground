namespace Game.Systems.Domain.WorldCognition.Model;

public readonly record struct WorldCell(
	float Presence,
	float Disturbance,
	float BearAffinity,
	float RavenAffinity,
	float SealAffinity);
