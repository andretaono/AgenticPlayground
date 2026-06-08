using Game.Systems.Foundation.Primitives;
using Game.Systems.Integration.Presentation.Ports;
using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;

namespace Game.UnityBridge.Input
{
	public sealed class UnityInputSource : IInputSource
	{
		private readonly AgentId _boundAgentId;
		private readonly PlayerFacingController _facing;

		public UnityInputSource(AgentId boundAgentId, PlayerFacingController facing)
		{
			_boundAgentId = boundAgentId;
			_facing = facing ?? throw new System.ArgumentNullException(nameof(facing));
		}

		public GameVector2 PollMovementInput(AgentId agentId)
		{
			if (!agentId.Equals(_boundAgentId))
				return GameVector2.Zero;

			var forwardBack = UnityEngine.Input.GetAxisRaw("Vertical");
			if (UnityEngine.Mathf.Abs(forwardBack) <= 1e-6f)
				return GameVector2.Zero;

			var direction = _facing.GetForwardSimDirection();
			if (forwardBack < 0f)
				direction = new GameVector2(-direction.X, -direction.Y);

			return direction.Normalized();
		}
	}
}
