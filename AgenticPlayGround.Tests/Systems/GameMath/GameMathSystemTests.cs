using Game.GameMath.Core;
using Game.GameMath.Core.Model;
using Xunit;

namespace AgenticPlayGround.Tests.Systems.GameMath;

public class GameMathSystemTests
{
    private readonly GameMathSystem _math = new();

    [Fact]
    public void Create_SetsComponents()
    {
        var v = _math.Create(1f, 2f, 3f);
        Assert.Equal(1f, v.X);
        Assert.Equal(2f, v.Y);
        Assert.Equal(3f, v.Z);
    }

    [Fact]
    public void Add_And_Subtract_WorkAsExpected()
    {
        var a = _math.Create(3f, 1f, 0f);
        var b = _math.Create(1f, 2f, 4f);

        var sum = _math.Add(a, b);
        var diff = _math.Subtract(a, b);

        Assert.Equal(new Vector3(4f, 3f, 4f), sum);
        Assert.Equal(new Vector3(2f, -1f, -4f), diff);
    }

    [Fact]
    public void Scale_MultipliesEachComponent()
    {
        var v = _math.Create(2f, -1f, 3f);
        var scaled = _math.Scale(v, 2f);
        Assert.Equal(new Vector3(4f, -2f, 6f), scaled);
    }

    [Fact]
    public void Magnitude_Of_3_0_4_Is_5()
    {
        var v = _math.Create(3f, 0f, 4f);
        Assert.Equal(5f, _math.Magnitude(v), precision: 5);
    }

    [Fact]
    public void Distance_MatchesExpectedValue()
    {
        var a = _math.Create(0f, 0f, 0f);
        var b = _math.Create(3f, 0f, 4f);
        Assert.Equal(5f, _math.Distance(a, b), precision: 5);
    }

    [Fact]
    public void Normalize_UnitVector_Unchanged()
    {
        var v = _math.Create(1f, 0f, 0f);
        var normalized = _math.Normalize(v);
        Assert.Equal(1f, _math.Magnitude(normalized), precision: 5);
        Assert.Equal(1f, normalized.X, precision: 5);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        var normalized = _math.Normalize(GameMathSystem.Zero);
        Assert.Equal(GameMathSystem.Zero, normalized);
    }

    [Fact]
    public void IsFinite_ReturnsFalse_ForNaNOrInfinity()
    {
        Assert.False(_math.IsFinite(_math.Create(float.NaN, 0f, 0f)));
        Assert.False(_math.IsFinite(_math.Create(0f, float.PositiveInfinity, 0f)));
        Assert.True(_math.IsFinite(_math.Create(1f, -2f, 3f)));
    }

    [Fact]
    public void Dot_ComputesExpectedValue()
    {
        var a = _math.Create(1f, 2f, 3f);
        var b = _math.Create(4f, 5f, 6f);
        Assert.Equal(32f, _math.Dot(a, b));
    }
}
