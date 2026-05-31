using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Controller;

internal sealed class AssemblyFactoryController : IAssemblyFactory
{
	public Assembly Create(int socketCount) => new(socketCount);
}
