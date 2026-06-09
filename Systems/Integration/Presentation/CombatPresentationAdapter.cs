using Game.Systems.Domain.EntityResource;
using Game.Systems.Domain.EntityResource.Ports;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Presentation;

public sealed class CombatPresentationAdapter : ITickable
{
	private readonly IWorldPresenter _presenter;
	private readonly IActorRegistry _actorRegistry;
	private readonly EntityResourceSystem _resources;
	private readonly CombatRuntimeServices _combatServices;
	private readonly GameSessionState _sessionState;
	private readonly float _worldUnitsPerTile;

	public CombatPresentationAdapter(
		IWorldPresenter presenter,
		IActorRegistry actorRegistry,
		EntityResourceSystem resources,
		CombatRuntimeServices combatServices,
		GameSessionState sessionState,
		float worldUnitsPerTile = 1f)
	{
		_presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_resources = resources ?? throw new ArgumentNullException(nameof(resources));
		_combatServices = combatServices ?? throw new ArgumentNullException(nameof(combatServices));
		_sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
		_worldUnitsPerTile = worldUnitsPerTile;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		foreach (var actor in _actorRegistry.Actors)
		{
			if (_sessionState.IsDead(actor.EntityId))
				continue;

			var health = _resources.Registry.TryGetDefinition<IHealthResourceDefinition>(actor.EntityId);
			if (health is not null)
			{
				_presenter.SyncActorHealth(
					actor.EntityId,
					health.CurrentAmount,
					health.MaximumAmount);
			}

			var forward = _combatServices.Orientation.GetForward(actor.EntityId);
			var yawDegrees = MathF.Atan2(forward.X, forward.Y) * (180f / MathF.PI);
			_presenter.SyncActorFacing(actor.EntityId, yawDegrees);
		}

		foreach (var swing in _combatServices.FeedbackStore.ConsumeRecentSwings())
		{
			_presenter.ShowAttackArc(
				swing.AttackerId,
				swing.Forward,
				swing.Range,
				swing.ArcDegrees,
				durationSeconds: 0.2f);
		}
	}
}
