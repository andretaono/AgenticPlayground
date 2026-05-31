using Game.Systems.Domain.ItemAssembly.Model;

namespace Game.Systems.Domain.ItemAssembly.Ports;

public interface IModifierResolver
{
	ResolvedModifierSet Resolve(Assembly assembly);
}
