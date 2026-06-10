using Game.Systems.Integration.Presentation;
using UnityEngine;
using UnityQuaternion = UnityEngine.Quaternion;
using UnityVector3 = UnityEngine.Vector3;

namespace Game.UnityBridge.Presentation
{
	public sealed class TopDownRpgCameraFollow
	{
		private float _cameraYawDegrees;
		private float _cameraYawVelocity;
		private UnityVector3 _positionVelocity;
		private float _pitchDegrees;
		private float _pitchVelocity;
		private float _lookYawVelocity;

		public TopDownCameraConfig Config { get; set; } = TopDownCameraConfig.Default;

		public void SnapTo(Transform player, Camera camera, float facingYawDegrees = 0f)
		{
			_cameraYawDegrees = facingYawDegrees;
			_cameraYawVelocity = 0f;
			_positionVelocity = UnityVector3.zero;
			_pitchDegrees = Config.PitchDegrees;
			_pitchVelocity = 0f;
			_lookYawVelocity = 0f;

			if (player == null || camera == null)
				return;

			var lookTarget = ComputeLookTarget(player.position, facingYawDegrees);
			var desiredPosition = ComputeCameraPosition(lookTarget, facingYawDegrees);
			var desiredRotation = UnityQuaternion.LookRotation(lookTarget - desiredPosition, UnityVector3.up);

			camera.transform.position = desiredPosition;
			camera.transform.rotation = desiredRotation;
			_pitchDegrees = NormalizePitch(desiredRotation.eulerAngles.x);
		}

		public void LateUpdate(Transform player, Camera camera, float facingYawDegrees)
		{
			if (player == null || camera == null)
				return;

			_cameraYawDegrees = UnityEngine.Mathf.SmoothDampAngle(
				_cameraYawDegrees,
				facingYawDegrees,
				ref _cameraYawVelocity,
				Config.YawSmoothTime);

			var lookTarget = ComputeLookTarget(player.position, _cameraYawDegrees);
			var desiredPosition = ComputeCameraPosition(lookTarget, _cameraYawDegrees);

			camera.transform.position = UnityVector3.SmoothDamp(
				camera.transform.position,
				desiredPosition,
				ref _positionVelocity,
				Config.PositionSmoothTime);

			var toTarget = lookTarget - camera.transform.position;
			if (toTarget.sqrMagnitude <= 1e-6f)
				return;

			var desiredRotation = UnityQuaternion.LookRotation(toTarget, UnityVector3.up);
			var desiredPitch = NormalizePitch(desiredRotation.eulerAngles.x);
			var desiredLookYaw = desiredRotation.eulerAngles.y;

			_pitchDegrees = UnityEngine.Mathf.SmoothDampAngle(
				_pitchDegrees,
				desiredPitch,
				ref _pitchVelocity,
				Config.RotationSmoothTime);

			var smoothedLookYaw = UnityEngine.Mathf.SmoothDampAngle(
				camera.transform.eulerAngles.y,
				desiredLookYaw,
				ref _lookYawVelocity,
				Config.RotationSmoothTime);

			camera.transform.rotation = UnityQuaternion.Euler(_pitchDegrees, smoothedLookYaw, 0f);
		}

		private UnityVector3 ComputeLookTarget(UnityVector3 playerPosition, float yawDegrees)
		{
			var forward = YawToForward(yawDegrees);
			return playerPosition
			       + UnityVector3.up * Config.LookHeight
			       + forward * Config.LookAhead;
		}

		private UnityVector3 ComputeCameraPosition(UnityVector3 lookTarget, float yawDegrees)
		{
			var orbitRotation = UnityQuaternion.Euler(Config.PitchDegrees, yawDegrees, 0f);
			return lookTarget + orbitRotation * (UnityVector3.back * Config.OrbitDistance);
		}

		private static UnityVector3 YawToForward(float yawDegrees)
		{
			var yawRadians = yawDegrees * UnityEngine.Mathf.Deg2Rad;
			return new UnityVector3(UnityEngine.Mathf.Sin(yawRadians), 0f, UnityEngine.Mathf.Cos(yawRadians));
		}

		private static float NormalizePitch(float pitchDegrees) =>
			pitchDegrees > 180f ? pitchDegrees - 360f : pitchDegrees;
	}
}
