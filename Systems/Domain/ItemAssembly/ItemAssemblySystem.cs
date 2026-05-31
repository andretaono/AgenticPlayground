using Game.Systems.Domain.ItemAssembly.Controller;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly;

/// <summary>
/// Root orchestrator: wires factories, assembly mutation, and modifier resolution.
/// </summary>
public sealed class ItemAssemblySystem : IItemAssemblySystem
{
	public IAssemblyFactory AssemblyFactory { get; }
	public IItemFactory ItemFactory { get; }
	public IAssemblyController Assembly { get; }
	public IModifierResolver Resolver { get; }

	public ItemAssemblySystem()
	{
		AssemblyFactory = new AssemblyFactoryController();
		ItemFactory = new ItemFactoryController();
		Assembly = new AssemblyController();
		Resolver = new ResolverController();
	}
}
