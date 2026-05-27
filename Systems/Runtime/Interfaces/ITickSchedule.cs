using System.Collections.Generic;

namespace Game.Runtime.Interfaces;

public interface ITickSchedule
{
    IReadOnlyList<TickEntry> Entries { get; }
}

