using Game.Systems.Foundation.GameMath.Core.Model;
using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using Game.UnityBridge.Input;

namespace Game.UnityBridge.Presentation
{
	public sealed class PlayerFacingProvider : IAgentFacingProvider
	{
		private readonly PlayerFacingController _facing;
		private readonly EntityId _playerEntityId;

		public PlayerFacingProvider(PlayerFacingController facing, EntityId playerEntityId)
		{
			_facing = facing ?? throw new System.ArgumentNullException(nameof(facing));
			_playerEntityId = playerEntityId;
		}

		public bool TryGetForward(EntityId entityId, out Vector2 forward)
		{
			if (!entityId.Equals(_playerEntityId))
			{
				forward = default;
				return false;
			}

			forward = _facing.GetForwardSimDirection();
			return true;
		}
	}
}
