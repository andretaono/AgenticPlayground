using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Combat;

public sealed class CombatClockAdapter : ITickable
{
	private readonly CombatRuntimeServices _services;

	public CombatClockAdapter(CombatRuntimeServices services) =>
		_services = services ?? throw new ArgumentNullException(nameof(services));

	public void Tick(float deltaTime) => _services.CurrentTime += deltaTime;
}
