using Game.Systems.Domain.AgentCombat.Ports;

namespace Game.Systems.Domain.AgentCombat.Model;

public sealed class AbilityContext
{
	public ICombatEntity Source { get; }

	public AbilityContext(ICombatEntity source)
	{
		Source = source ?? throw new ArgumentNullException(nameof(source));
	}
}
