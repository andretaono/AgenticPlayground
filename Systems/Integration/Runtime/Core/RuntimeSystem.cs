using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Runtime.Core;

/// <summary>
/// Engine-agnostic runtime orchestrator. Advances an injected, ordered schedule of tickables.
/// </summary>
public sealed class RuntimeSystem
{
    private readonly ITickSchedule _schedule;
    private readonly ITickable[] _orderedTickables;

    public IReadOnlyList<ITickable> Tickables => _orderedTickables;

    public RuntimeSystem(ITickSchedule schedule)
    {
        _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        if (_schedule.Entries == null) throw new ArgumentException("Schedule entries cannot be null.", nameof(schedule));

        _orderedTickables = _schedule.Entries
            .OrderBy(e => e.Order)
            .Select(e => e.Tickable ?? throw new ArgumentException("Tickable cannot be null.", nameof(schedule)))
            .ToArray();
    }

    public void Tick(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

        for (var i = 0; i < _orderedTickables.Length; i++)
            _orderedTickables[i].Tick(deltaTime);
    }
}

