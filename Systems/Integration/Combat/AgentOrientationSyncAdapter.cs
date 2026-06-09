using Game.Systems.Domain.AgentMovement;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Actors;
using Game.Systems.Integration.Presentation.Ports;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Combat;

public sealed class AgentOrientationSyncAdapter : ITickable
{
	private readonly AgentOrientationStore _orientation;
	private readonly AgentMovementSystem _movement;
	private readonly IActorRegistry _actorRegistry;
	private readonly IAgentFacingProvider? _facingProvider;
	private readonly IReadOnlyDictionary<EntityId, EntityId> _faceTargetByEntity;

	public AgentOrientationSyncAdapter(
		AgentOrientationStore orientation,
		AgentMovementSystem movement,
		IActorRegistry actorRegistry,
		IAgentFacingProvider? facingProvider = null,
		IReadOnlyDictionary<EntityId, EntityId>? faceTargetByEntity = null)
	{
		_orientation = orientation ?? throw new ArgumentNullException(nameof(orientation));
		_movement = movement ?? throw new ArgumentNullException(nameof(movement));
		_actorRegistry = actorRegistry ?? throw new ArgumentNullException(nameof(actorRegistry));
		_facingProvider = facingProvider;
		_faceTargetByEntity = faceTargetByEntity ?? new Dictionary<EntityId, EntityId>();
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;

		foreach (var actor in _actorRegistry.Actors)
		{
			var entityId = actor.EntityId;
			if (_facingProvider is not null &&
			    _facingProvider.TryGetForward(entityId, out var providerForward))
			{
				_orientation.SetForward(entityId, providerForward);
				continue;
			}

			if (_faceTargetByEntity.TryGetValue(entityId, out var targetId))
			{
				var sourcePosition = ToVector2(_movement.Input.GetPosition(entityId));
				var targetPosition = ToVector2(_movement.Input.GetPosition(targetId));
				var toTarget = Subtract(targetPosition, sourcePosition);
				if (toTarget.X * toTarget.X + toTarget.Y * toTarget.Y > 1e-8f)
				{
					_orientation.SetForward(entityId, toTarget.Normalized());
					continue;
				}
			}

			var velocity = _movement.Input.GetVelocity(entityId);
			var planarVelocity = new Vector2(velocity.X, velocity.Y);
			if (planarVelocity.X * planarVelocity.X + planarVelocity.Y * planarVelocity.Y > 1e-8f)
				_orientation.SetForward(entityId, planarVelocity.Normalized());
		}
	}

	private static Vector2 ToVector2(Game.Systems.Foundation.GameMath.Interfaces.IVector3 position) =>
		new(position.X, position.Y);

	private static Vector2 Subtract(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
}
