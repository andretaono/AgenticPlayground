using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Runtime;

public sealed class DefaultTickSchedule : ITickSchedule
{
	public DefaultTickSchedule(IReadOnlyList<TickEntry> entries) =>
		Entries = entries ?? throw new ArgumentNullException(nameof(entries));

	public IReadOnlyList<TickEntry> Entries { get; }
}
