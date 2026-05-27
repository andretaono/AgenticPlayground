using Game.Foundation.GameMath.Example;
using Game.Inventory.Example;
using Game.AgentMovement.Example;

GameMathDemo.Run();
Console.WriteLine();
InventoryDemo.Run();
Console.WriteLine();
AgentMovementDemo.Run();

Console.WriteLine();
Console.WriteLine("Boot runtime tick once...");
var runtime = Game.Boot.CreateRuntime();
runtime.Tick(1f / 60f);