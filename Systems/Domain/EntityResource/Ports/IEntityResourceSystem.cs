namespace Game.Systems.Domain.EntityResource.Ports;

public interface IEntityResourceSystem
{
	IEntityResourceRegistry Registry { get; }
	IEntityResourceSimulation Simulation { get; }
}
