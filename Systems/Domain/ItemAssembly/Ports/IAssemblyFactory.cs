using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IAssemblyFactory
{
	Assembly Create(int socketCount);
}
