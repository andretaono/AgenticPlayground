using Game.Systems.Domain.WorldCognition.Model;
using Game.Systems.Domain.WorldCognition.Ports;
using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Runtime.Interfaces;

namespace Game.Systems.Integration.Adapters;

/// <summary>
/// Records player movement and activity into the world cognition presence map.
/// </summary>
public sealed class PlayerPresenceAdapter : ITickable
{
	private readonly IWorldCognitionController _cognition;
	private readonly Func<EntityId, Vector2> _getPlayerPosition;
	private readonly EntityId _playerEntityId;
	private readonly float _presenceAmount;

	public PlayerPresenceAdapter(
		IWorldCognitionController cognition,
		EntityId playerEntityId,
		Func<EntityId, Vector2> getPlayerPosition,
		bool sprinting = false)
	{
		_cognition = cognition ?? throw new ArgumentNullException(nameof(cognition));
		_playerEntityId = playerEntityId;
		_getPlayerPosition = getPlayerPosition ?? throw new ArgumentNullException(nameof(getPlayerPosition));
		_presenceAmount = sprinting
			? WorldCognitionContributions.Presence.Sprinting
			: WorldCognitionContributions.Presence.Movement;
	}

	public void Tick(float deltaTime)
	{
		_ = deltaTime;
		var position = _getPlayerPosition(_playerEntityId);
		_cognition.AddPresence(position, _presenceAmount);
	}
}
