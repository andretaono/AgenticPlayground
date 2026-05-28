using System;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.GameMath.Interfaces;

namespace Game.Systems.Foundation.GameMath.Core;

/// <summary>
/// Entry point for engine-agnostic math operations.
/// </summary>
public sealed class GameMathSystem : IGameMath
{
    public static readonly Vector3 Zero = new(0f, 0f, 0f);

    public Vector3 Create(float x, float y, float z) => new(x, y, z);

    public Vector3 Add(IVector3 a, IVector3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public Vector3 Subtract(IVector3 a, IVector3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public Vector3 Scale(IVector3 v, float scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public float Dot(IVector3 a, IVector3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public float MagnitudeSquared(IVector3 v) => Dot(v, v);

    public float Magnitude(IVector3 v) => MathF.Sqrt(MagnitudeSquared(v));

    public float Distance(IVector3 a, IVector3 b) => Magnitude(Subtract(a, b));

    public Vector3 Normalize(IVector3 v)
    {
        var magnitudeSquared = MagnitudeSquared(v);
        if (magnitudeSquared <= 0f)
            return Zero;

        var inverseMagnitude = 1f / MathF.Sqrt(magnitudeSquared);
        return Scale(v, inverseMagnitude);
    }

    public bool IsFinite(IVector3 v) =>
        IsFiniteComponent(v.X) && IsFiniteComponent(v.Y) && IsFiniteComponent(v.Z);

    private static bool IsFiniteComponent(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);
}

