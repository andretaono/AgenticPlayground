using Game.Systems.Integration.Runtime.Core;

namespace Game.Systems.Integration.Runtime;

public sealed class GameRuntime
{
	public GameRuntime(RuntimeSystem runtime, GameSystems systems)
	{
		Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
		Systems = systems ?? throw new ArgumentNullException(nameof(systems));
	}

	public RuntimeSystem Runtime { get; }
	public GameSystems Systems { get; }

	public void Tick(float deltaTime) => Runtime.Tick(deltaTime);
}
