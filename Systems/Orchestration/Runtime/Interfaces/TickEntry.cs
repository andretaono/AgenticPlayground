namespace Game.Systems.Orchestration.Runtime.Interfaces;

public readonly record struct TickEntry(ITickable Tickable, int Order = 0);

