using Game.Foundation.GameMath.Interfaces;

namespace Game.Foundation.GameMath.Core.Model;

/// <summary>
/// Immutable engine-agnostic 3D vector.
/// </summary>
public readonly struct Vector3 : IVector3
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString() => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
}

