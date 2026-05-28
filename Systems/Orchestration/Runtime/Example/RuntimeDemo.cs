using Game.Systems.Orchestration.Runtime.Core;
using Game.Systems.Orchestration.Runtime.Interfaces;

namespace Game.Systems.Orchestration.Runtime.Example;

public static class RuntimeDemo
{
    private sealed class DemoSchedule : ITickSchedule
    {
        public IReadOnlyList<TickEntry> Entries { get; }
        public DemoSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
    }

    private sealed class CounterTickable : ITickable
    {
        private int _ticks;
        public void Tick(float deltaTime)
        {
            _ticks++;
            Console.WriteLine($"Tick {_ticks} (dt={deltaTime:0.000})");
        }
    }

    public static void Run()
    {
        var entries = new[]
        {
            new TickEntry(new CounterTickable(), Order: 10)
        };

        var runtime = new RuntimeSystem(new DemoSchedule(entries));

        runtime.Tick(1f / 60f);
        runtime.Tick(1f / 60f);
        runtime.Tick(1f / 60f);
    }
}

