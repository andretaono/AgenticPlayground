namespace Game.Systems.Domain.EntityResource.Ports;

public interface IEntityResourceSystem
{
	IEntityResourceRegistry Registry { get; }
	IEntityResourceController Resource { get; }
	IEntityResourceSimulation Simulation { get; }
}
