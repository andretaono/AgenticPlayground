using UnityEngine;
using Vector2 = Game.Systems.Foundation.GameMath.Core.Model.Vector2;

namespace Game.UnityBridge.Input
{
	public sealed class PlayerFacingController
	{
		public float FacingYawDegrees { get; private set; }

		public void Snap(float yawDegrees = 0f) => FacingYawDegrees = yawDegrees;

		public void ApplyTurnInput(float horizontalAxis, float deltaTime, float turnSpeedDegrees)
		{
			if (Mathf.Abs(horizontalAxis) <= 1e-6f || deltaTime <= 0f)
				return;

			FacingYawDegrees += horizontalAxis * turnSpeedDegrees * deltaTime;
		}

		public Vector2 GetForwardSimDirection()
		{
			var yawRadians = FacingYawDegrees * Mathf.Deg2Rad;
			return new Vector2(Mathf.Sin(yawRadians), Mathf.Cos(yawRadians));
		}
	}
}
