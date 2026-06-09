using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Combat;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Presentation;

public sealed class WorldPresentationAdapter : ITickable
{
	private readonly IWorldPresenter _presenter;
	private readonly IActorRegistry _actorRegistry;
	private readonly AgentMovementSystem _movement;
	private readonly GameSessionState? _sessionState;

	public WorldPresentationAdapter(
		IWorldPresenter presenter,
		IActorRegistry actorRegistry,
		AgentMovementSystem movement,
		GameSessionState? sessionState = null)
	{
		_presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
		_sessionState = sessionState;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		foreach (var actor in _actorRegistry.Actors)
		{
			if (_sessionState is not null && _sessionState.IsDead(actor.EntityId))
				continue;

			var pos = _movement.Input.GetPosition(actor.EntityId);
			_presenter.SyncActorPosition(actor.EntityId, new Vector2(pos.X, pos.Y));
		}
	}
}
