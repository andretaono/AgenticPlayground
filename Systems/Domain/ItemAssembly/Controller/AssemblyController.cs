using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Controller;

internal sealed class AssemblyController : IAssemblyController
{
	public void AddItem(Assembly assembly, int socketIndex, Item item)
	{
		if (assembly is null) throw new ArgumentNullException(nameof(assembly));
		if (item is null) throw new ArgumentNullException(nameof(item));

		if (assembly.GetItem(socketIndex) is not null)
			throw new InvalidOperationException($"Socket '{socketIndex}' is already occupied.");

		assembly.SetItem(socketIndex, item);
	}

	public void RemoveItem(Assembly assembly, int socketIndex)
	{
		if (assembly is null) throw new ArgumentNullException(nameof(assembly));

		if (assembly.GetItem(socketIndex) is null)
			throw new InvalidOperationException($"Socket '{socketIndex}' is empty.");

		assembly.SetItem(socketIndex, null);
	}

	public Item? GetItem(Assembly assembly, int socketIndex)
	{
		if (assembly is null) throw new ArgumentNullException(nameof(assembly));
		return assembly.GetItem(socketIndex);
	}
}
