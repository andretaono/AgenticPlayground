namespace Game.Systems.Integration.Runtime.Interfaces;

public interface ITickSchedule
{
    IReadOnlyList<TickEntry> Entries { get; }
}

