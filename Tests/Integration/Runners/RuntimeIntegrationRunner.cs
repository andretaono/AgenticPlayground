using Game.Systems.Integration.Runtime.Core;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Tests.Integration.Runners;

public sealed class RuntimeIntegrationRunner
{
	public RuntimeIntegrationResult Run()
	{
		var counter = new CounterTickable();
		var runtime = new RuntimeSystem(new FixedSchedule(new[]
		{
			new TickEntry(counter, Order: 10)
		}));

		const float deltaTime = 1f / 60f;
		runtime.Tick(deltaTime);
		runtime.Tick(deltaTime);
		runtime.Tick(deltaTime);

		return new RuntimeIntegrationResult(counter.TickCount, deltaTime);
	}

	private sealed class FixedSchedule : ITickSchedule
	{
		public FixedSchedule(IReadOnlyList<TickEntry> entries) => Entries = entries;
		public IReadOnlyList<TickEntry> Entries { get; }
	}

	private sealed class CounterTickable : ITickable
	{
		public int TickCount { get; private set; }

		public void Tick(float deltaTime)
		{
			_ = deltaTime;
			TickCount++;
		}
	}
}

public sealed record RuntimeIntegrationResult(int TickCount, float DeltaTime);
