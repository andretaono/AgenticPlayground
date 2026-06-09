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

		public SettingsConfig Settings { get; set; } = SettingsConfig.Default;

		public void SnapTo(Transform player, Camera camera, float facingYawDegrees = 0f)
		{
			_cameraYawDegrees = facingYawDegrees;
			_cameraYawVelocity = 0f;
			_positionVelocity = UnityVector3.zero;
			_pitchDegrees = Settings.PitchDegrees;
			_pitchVelocity = 0f;
			_lookYawVelocity = 0f;

			if (player == null || camera == null)
				return;

			player.rotation = UnityQuaternion.Euler(0f, facingYawDegrees, 0f);

			var settings = Settings;
			var lookTarget = ComputeLookTarget(player.position, facingYawDegrees, settings);
			var desiredPosition = ComputeCameraPosition(lookTarget, facingYawDegrees, settings);
			var desiredRotation = UnityQuaternion.LookRotation(lookTarget - desiredPosition, UnityVector3.up);

			camera.transform.position = desiredPosition;
			camera.transform.rotation = desiredRotation;
			_pitchDegrees = NormalizePitch(desiredRotation.eulerAngles.x);
		}

		public void LateUpdate(Transform player, Camera camera, float facingYawDegrees)
		{
			if (player == null || camera == null)
				return;

			var settings = Settings;
			player.rotation = UnityQuaternion.Euler(0f, facingYawDegrees, 0f);

			_cameraYawDegrees = UnityEngine.Mathf.SmoothDampAngle(
				_cameraYawDegrees,
				facingYawDegrees,
				ref _cameraYawVelocity,
				settings.YawSmoothTime);

			var lookTarget = ComputeLookTarget(player.position, _cameraYawDegrees, settings);
			var desiredPosition = ComputeCameraPosition(lookTarget, _cameraYawDegrees, settings);

			camera.transform.position = UnityVector3.SmoothDamp(
				camera.transform.position,
				desiredPosition,
				ref _positionVelocity,
				settings.PositionSmoothTime);

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
				settings.RotationSmoothTime);

			var smoothedLookYaw = UnityEngine.Mathf.SmoothDampAngle(
				camera.transform.eulerAngles.y,
				desiredLookYaw,
				ref _lookYawVelocity,
				settings.RotationSmoothTime);

			camera.transform.rotation = UnityQuaternion.Euler(_pitchDegrees, smoothedLookYaw, 0f);
		}

		private static UnityVector3 ComputeLookTarget(UnityVector3 playerPosition, float yawDegrees, SettingsConfig settings)
		{
			var forward = YawToForward(yawDegrees);
			return playerPosition
				+ UnityVector3.up * settings.LookHeight
				+ forward * settings.LookAhead;
		}

		private static UnityVector3 ComputeCameraPosition(UnityVector3 lookTarget, float yawDegrees, SettingsConfig settings)
		{
			var orbitRotation = UnityQuaternion.Euler(settings.PitchDegrees, yawDegrees, 0f);
			return lookTarget + orbitRotation * (UnityVector3.back * settings.OrbitDistance);
		}

		private static UnityVector3 YawToForward(float yawDegrees)
		{
			var yawRadians = yawDegrees * UnityEngine.Mathf.Deg2Rad;
			return new UnityVector3(UnityEngine.Mathf.Sin(yawRadians), 0f, UnityEngine.Mathf.Cos(yawRadians));
		}

		private static float NormalizePitch(float pitchDegrees) =>
			pitchDegrees > 180f ? pitchDegrees - 360f : pitchDegrees;

		public sealed class SettingsConfig
		{
			public float OrbitDistance = 16f;
			public float PitchDegrees = 52f;
			public float LookHeight = 0.75f;
			public float LookAhead = 0.25f;
			public float YawSmoothTime = 0.15f;
			public float PositionSmoothTime = 0.1f;
			public float RotationSmoothTime = 0.08f;

			public static SettingsConfig Default { get; } = new();
		}
	}
}
