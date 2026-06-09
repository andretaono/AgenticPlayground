using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Combat;

public sealed class VitalityMonitorAdapter : ITickable
{
	private readonly EntityResourceSystem _resources;
	private readonly GameSessionState _sessionState;
	private readonly EntityId _playerEntityId;
	private readonly IActorRegistry _actorRegistry;
	private readonly VitalityCleanupServices? _cleanup;

	public VitalityMonitorAdapter(
		EntityResourceSystem resources,
		GameSessionState sessionState,
		EntityId playerEntityId,
		IActorRegistry actorRegistry,
		VitalityCleanupServices? cleanup = null)
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

		foreach (var actor in _actorRegistry.Actors)
		{
			if (_sessionState.IsDead(actor.EntityId))
				continue;

			var health = _resources.Registry.TryGetDefinition<IHealthResourceDefinition>(actor.EntityId);
			if (health is null || !health.IsDepleted)
				continue;

			HandleDeath(actor);
		}
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

public sealed class VitalityCleanupServices
{
	public VitalityCleanupServices(
		IActorRegistry actorRegistry,
		Game.Systems.Domain.AgentMovement.AgentMovementSystem movement,
		Game.Systems.Domain.AgentCommand.AgentCommandSystem commandSystem,
		Game.Systems.Domain.AgentCombat.Ports.ICombatEntityRegistry combatRegistry,
		IWorldPresenter? presenter = null)
	{
		ActorRegistry = actorRegistry;
		Movement = movement;
		CommandSystem = commandSystem;
		CombatRegistry = combatRegistry;
		Presenter = presenter;
	}

	public IActorRegistry ActorRegistry { get; }
	public Game.Systems.Domain.AgentMovement.AgentMovementSystem Movement { get; }
	public Game.Systems.Domain.AgentCommand.AgentCommandSystem CommandSystem { get; }
	public Game.Systems.Domain.AgentCombat.Ports.ICombatEntityRegistry CombatRegistry { get; }
	public IWorldPresenter? Presenter { get; }

	public void RemoveDeadActor(ActorHandle actor)
	{
		if (CombatRegistry.TryGet(actor.EntityId, out var combatEntity))
			CombatRegistry.Unregister(combatEntity);

		Movement.Registry.RemoveAgent(actor.EntityId);
		CommandSystem.UnregisterAgent(actor.AgentId);
		ActorRegistry.RemoveActor(actor);
		Presenter?.RemoveActor(actor.EntityId);
	}
}
