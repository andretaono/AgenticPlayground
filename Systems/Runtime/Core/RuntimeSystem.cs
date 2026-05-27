using System;
using System.Collections.Generic;
using Game.Runtime.Interfaces;

namespace Game.Runtime.Core;

/// <summary>
/// Engine-agnostic runtime orchestrator. Owns update order and advances registered tickables.
/// </summary>
public sealed class RuntimeSystem
{
    private readonly List<ITickable> _tickables = new();

    public IReadOnlyList<ITickable> Tickables => _tickables.AsReadOnly();

    public void Register(ITickable tickable)
    {
        if (tickable == null) throw new ArgumentNullException(nameof(tickable));
        _tickables.Add(tickable);
    }

    public bool Unregister(ITickable tickable) => _tickables.Remove(tickable);

    public void Tick(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be finite and non-negative.");

        for (var i = 0; i < _tickables.Count; i++)
            _tickables[i].Tick(deltaTime);
    }
}

