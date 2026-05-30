namespace Game.Systems.Domain.EntityResource.Ports;

public interface IEntityResourceSimulation
{
	void AdvanceSimulation(float deltaTime);
}
