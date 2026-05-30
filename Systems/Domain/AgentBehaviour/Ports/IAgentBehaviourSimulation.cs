namespace Game.Systems.Domain.AgentBehaviour.Ports;

public interface IAgentBehaviourSimulation
{
	void Tick(float deltaTime);
}
