namespace Game.Systems.Foundation.GameMath.Core.Model;

public readonly struct Vector2
{
    public float X { get; init; }
    public float Y { get; init; }

    public Vector2(float x, float y) => (X, Y) = (x, y);

    public static Vector2 Zero => new(0f, 0f);

    public float Magnitude() => MathF.Sqrt(X * X + Y * Y);

    public Vector2 Normalized()
    {
        var m = Magnitude();
        return m <= 1e-6f ? Zero : new Vector2(X / m, Y / m);
    }

    public override string ToString() => $"({X:F2}, {Y:F2})";
}
