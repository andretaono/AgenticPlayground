using System.Collections.Generic;

namespace Game.Systems.Orchestration.Runtime.Interfaces;

public interface ITickSchedule
{
    IReadOnlyList<TickEntry> Entries { get; }
}

