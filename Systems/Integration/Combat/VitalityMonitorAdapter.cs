using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Combat;

public sealed class VitalityMonitorAdapter : ITickable
{
	private readonly EntityResourceSystem _resources;
	private readonly GameSessionState _sessionState;
	private readonly EntityId _playerEntityId;
	private readonly IActorRegistry _actorRegistry;
	private readonly IActorLifecycleCleanup? _cleanup;

	public VitalityMonitorAdapter(
		EntityResourceSystem resources,
		GameSessionState sessionState,
		EntityId playerEntityId,
		IActorRegistry actorRegistry,
		IActorLifecycleCleanup? cleanup = null)
	{
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
		_playerEntityId = playerEntityId;
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_cleanup = cleanup;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		List<ActorHandle>? pendingDeaths = null;

		foreach (var actor in _actorRegistry.Actors)
		{
			if (_sessionState.IsDead(actor.EntityId))
				continue;

			var health = _resources.Registry.TryGetDefinition<IHealthResourceDefinition>(actor.EntityId);
			if (health is null || !health.IsDepleted)
				continue;

			(pendingDeaths ??= new List<ActorHandle>()).Add(actor);
		}

		if (pendingDeaths is null)
			return;

		foreach (var actor in pendingDeaths)
			HandleDeath(actor);
	}

	private void HandleDeath(ActorHandle actor)
	{
		_sessionState.MarkEntityDead(actor.EntityId);

		if (actor.EntityId.Equals(_playerEntityId))
		{
			_sessionState.MarkPlayerDead();
			return;
		}

		_cleanup?.RemoveDeadActor(actor);
	}
}
