using System;
using Game.Runtime.Core;
using Game.Runtime.Interfaces;

namespace Game.Runtime.Example;

public static class RuntimeDemo
{
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
        var runtime = new RuntimeSystem();
        runtime.Register(new CounterTickable());

        runtime.Tick(1f / 60f);
        runtime.Tick(1f / 60f);
        runtime.Tick(1f / 60f);
    }
}

