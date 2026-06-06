namespace Game.Systems.Domain.WorldCognition.Ports;

public interface IWorldCognitionSystem
{
	IWorldCognitionController Cognition { get; }
	IWorldCognitionSimulation Simulation { get; }
}
