using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;

namespace Game.UnityBridge.Input
{
	public sealed class UnityInputSource : IInputSource
	{
		private readonly PlayerFacingController _facing;
		private AgentId? _boundAgentId;

		public UnityInputSource(PlayerFacingController facing)
		{
			_facing = facing ?? throw new System.ArgumentNullException(nameof(facing));
		}

		public UnityInputSource(AgentId boundAgentId, PlayerFacingController facing)
			: this(facing)
		{
			_boundAgentId = boundAgentId;
		}

		public void Bind(AgentId agentId) => _boundAgentId = agentId;

		public GameVector2 PollMovementInput(AgentId agentId)
		{
			if (_boundAgentId is { } bound && !agentId.Equals(bound))
				return GameVector2.Zero;

			var forwardBack = UnityEngine.Input.GetAxisRaw("Vertical");
			if (UnityEngine.Mathf.Abs(forwardBack) <= 1e-6f)
				return GameVector2.Zero;

			var direction = _facing.GetForwardSimDirection();
			if (forwardBack < 0f)
				direction = new GameVector2(-direction.X, -direction.Y);

			return direction.Normalized();
		}

		public bool PollAttackInput(AgentId agentId)
		{
			if (_boundAgentId is { } bound && !agentId.Equals(bound))
				return false;

			return UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) ||
			       UnityEngine.Input.GetMouseButtonDown(0);
		}
	}
}
