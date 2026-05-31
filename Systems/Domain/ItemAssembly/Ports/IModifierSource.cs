using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IModifierSource
{
	IReadOnlyList<Modifier> Modifiers { get; }
}
