using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IItemFactory
{
	Item Create(IEnumerable<Modifier> modifiers);
}
