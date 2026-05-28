using Game.Systems.Foundation.GameMath.Core;

namespace Game.Systems.Foundation.GameMath.Example;

public static class GameMathDemo
{
    public static void Run()
    {
        var math = new GameMathSystem();

        var a = math.Create(3f, 0f, 4f);
        var b = math.Create(1f, 2f, 0f);

        Console.WriteLine($"a = {a}");
        Console.WriteLine($"b = {b}");
        Console.WriteLine($"a + b = {math.Add(a, b)}");
        Console.WriteLine($"a - b = {math.Subtract(a, b)}");
        Console.WriteLine($"|a| = {math.Magnitude(a):0.###}");
        Console.WriteLine($"normalize(a) = {math.Normalize(a)}");
        Console.WriteLine($"distance(a, b) = {math.Distance(a, b):0.###}");
        Console.WriteLine($"isFinite(a) = {math.IsFinite(a)}");
        Console.WriteLine($"isFinite(NaN vector) = {math.IsFinite(math.Create(float.NaN, 0f, 0f))}");
    }
}

