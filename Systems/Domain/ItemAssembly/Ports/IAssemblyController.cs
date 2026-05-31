using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IAssemblyController
{
	void AddItem(Assembly assembly, int socketIndex, Item item);

	void RemoveItem(Assembly assembly, int socketIndex);

	Item? GetItem(Assembly assembly, int socketIndex);
}
