using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IAssemblyStorage
{
	int SocketCount { get; }

	Item? GetItem(int socketIndex);
}
