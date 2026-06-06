using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Presentation;

public sealed class WorldPresentationAdapter : ITickable
{
	private readonly IWorldPresenter _presenter;
	private readonly IActorRegistry _actorRegistry;
	private readonly AgentMovementSystem _movement;

	public WorldPresentationAdapter(
		IWorldPresenter presenter,
		IActorRegistry actorRegistry,
		AgentMovementSystem movement)
	{
		_presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		foreach (var actor in _actorRegistry.Actors)
		{
			var pos = _movement.Input.GetPosition(actor.EntityId);
			_presenter.SyncActorPosition(actor.EntityId, new Vector2(pos.X, pos.Y));
		}
	}
}
