using Game.Systems.Domain.ItemAssembly.Model;
using Game.Systems.Domain.ItemAssembly.Ports;

namespace Game.Systems.Domain.ItemAssembly.Controller;

internal sealed class ItemFactoryController : IItemFactory
{
	private int _nextItemId;

	public Item Create(IEnumerable<Modifier> modifiers)
	{
		if (modifiers is null)
			throw new ArgumentNullException(nameof(modifiers));

		var modifierList = modifiers.ToList().AsReadOnly();
		var id = new ItemId(Interlocked.Increment(ref _nextItemId));
		return new Item(id, modifierList);
	}
}
