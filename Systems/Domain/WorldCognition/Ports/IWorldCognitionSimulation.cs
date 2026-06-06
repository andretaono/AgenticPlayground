namespace Game.Systems.Domain.WorldCognition.Ports;

public interface IWorldCognitionSimulation
{
	void AdvanceSimulation(float deltaTime);
}
