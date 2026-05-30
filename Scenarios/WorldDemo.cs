using System.Text;
using Game.Scenarios.Core.Interfaces;
using Game.Systems.Domain.World.Model;
using Game.Systems.Domain.World.Ports;
using Game.Systems.Integration.Adapters;
using Game.Systems.Domain.World;

namespace Game.Scenarios;

public class WorldDemo : IScenario
{
    public string Name => "world-demo";

    public void Run()
    {
        // Build a small map (10x6) using TileId strings
        var w = 10;
        var h = 6;
        var map = new TileId[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                map[x, y] = new TileId("ground");

        // border walls
        for (int x = 0; x < w; x++) { map[x, 0] = new TileId("wall"); map[x, h - 1] = new TileId("wall"); }
        for (int y = 0; y < h; y++) { map[0, y] = new TileId("wall"); map[w - 1, y] = new TileId("wall"); }

        // small water pool
        map[4, 2] = new TileId("water");
        map[5, 2] = new TileId("water");
        map[4, 3] = new TileId("water");

        IWorldDataSource ds = new InMemoryWorldDataSource(map);
        IWorldSystem world = new WorldSystem(ds);

        // Adapters
        var visual = new DefaultTileVisualMapper();
        var rules = new DefaultTileRulesProvider();

        Console.WriteLine("WorldDemo: printed map (W=Wall, .=Ground, ~ = Water)\n");
        PrintMap(world, w, h, visual);

        var center = new WorldPosition(5, 2);
        Console.WriteLine($"\nNeighborhood around {center} (radius=1):");
        var neigh = world.GetNeighborhood(center, 1);
        foreach (var t in neigh)
        {
            var label = visual.MapToLabel(t.Id);
            var tr = rules.GetRules(t.Id);
            Console.WriteLine($" - {t.Position}: {t.Id} ({label}) Rules: {tr}");
        }

        var outOfBounds = new WorldPosition(-1, -1);
        Console.WriteLine($"\nIs {outOfBounds} in bounds? {world.IsInBounds(outOfBounds)}");
        Console.WriteLine($"TileId at {center}: {world.GetTileId(center)}");
    }

    private static void PrintMap(IWorldSystem world, int width, int height, DefaultTileVisualMapper visual)
    {
        var sb = new StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var id = world.GetTileId(new WorldPosition(x, y));
                var ch = visual.MapToChar(id);
                sb.Append(ch);
            }
            sb.AppendLine();
        }
        Console.WriteLine(sb.ToString());
    }
}