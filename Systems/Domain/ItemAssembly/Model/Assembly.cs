using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Model;

public sealed class Assembly : IAssemblyStorage
{
	internal Item?[] Sockets { get; }

	internal Assembly(int socketCount)
	{
		if (socketCount < 0)
			throw new ArgumentOutOfRangeException(nameof(socketCount), "Socket count must be non-negative.");

		SocketCount = socketCount;
		Sockets = new Item?[socketCount];
	}

	public int SocketCount { get; }

	public Item? GetItem(int socketIndex)
	{
		ValidateSocketIndex(socketIndex);
		return Sockets[socketIndex];
	}

	internal void SetItem(int socketIndex, Item? item)
	{
		ValidateSocketIndex(socketIndex);
		Sockets[socketIndex] = item;
	}

	private void ValidateSocketIndex(int socketIndex)
	{
		if (socketIndex < 0 || socketIndex >= SocketCount)
			throw new ArgumentOutOfRangeException(nameof(socketIndex), "Socket index is out of range.");
	}
}
