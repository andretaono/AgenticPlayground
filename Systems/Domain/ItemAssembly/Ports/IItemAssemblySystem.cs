namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IItemAssemblySystem
{
	IAssemblyFactory AssemblyFactory { get; }
	IItemFactory ItemFactory { get; }
	IAssemblyController Assembly { get; }
	IModifierResolver Resolver { get; }
}
