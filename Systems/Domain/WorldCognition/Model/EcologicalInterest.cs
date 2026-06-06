namespace Game.Systems.Domain.WorldCognition.Model;

public readonly record struct EcologicalInterest(
	float Bear,
	float Raven,
	float Seal,
	AffinityType DominantInterest);
