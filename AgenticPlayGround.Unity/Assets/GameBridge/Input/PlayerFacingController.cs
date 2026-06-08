using GameVector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;

namespace Game.UnityBridge.Input
{
	public sealed class PlayerFacingController
	{
		public float FacingYawDegrees { get; private set; }

		public void Snap(float yawDegrees = 0f) => FacingYawDegrees = yawDegrees;

		public void ApplyTurnInput(float horizontalAxis, float deltaTime, float turnSpeedDegrees)
		{
			if (UnityEngine.Mathf.Abs(horizontalAxis) <= 1e-6f || deltaTime <= 0f)
				return;

			FacingYawDegrees += horizontalAxis * turnSpeedDegrees * deltaTime;
		}

		public GameVector2 GetForwardSimDirection()
		{
			var yawRadians = FacingYawDegrees * UnityEngine.Mathf.Deg2Rad;
			return new GameVector2(UnityEngine.Mathf.Sin(yawRadians), UnityEngine.Mathf.Cos(yawRadians));
		}
	}
}
