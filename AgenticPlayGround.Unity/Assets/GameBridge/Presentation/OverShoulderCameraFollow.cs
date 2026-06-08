using UnityEngine;
using UnityQuaternion = UnityEngine.Quaternion;
using UnityVector3 = UnityEngine.Vector3;

namespace Game.UnityBridge.Presentation
{
	public sealed class OverShoulderCameraFollow
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
			_pitchDegrees = 0f;
			_pitchVelocity = 0f;
			_lookYawVelocity = 0f;

			if (player == null || camera == null)
				return;

			player.rotation = UnityQuaternion.Euler(0f, facingYawDegrees, 0f);

			var settings = Settings;
			var desiredPosition = ComputeCameraPosition(player.position, facingYawDegrees, settings);
			var lookTarget = ComputeLookTarget(player.position, facingYawDegrees, settings);
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

			var desiredPosition = ComputeCameraPosition(player.position, _cameraYawDegrees, settings);
			camera.transform.position = UnityVector3.SmoothDamp(
				camera.transform.position,
				desiredPosition,
				ref _positionVelocity,
				settings.PositionSmoothTime);

			var lookTarget = ComputeLookTarget(player.position, _cameraYawDegrees, settings);
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

		private static UnityVector3 ComputeCameraPosition(UnityVector3 playerPosition, float yawDegrees, SettingsConfig settings)
		{
			var yawRadians = yawDegrees * UnityEngine.Mathf.Deg2Rad;
			var forward = new UnityVector3(UnityEngine.Mathf.Sin(yawRadians), 0f, UnityEngine.Mathf.Cos(yawRadians));
			var right = UnityVector3.Cross(UnityVector3.up, forward).normalized;
			return playerPosition
				+ UnityVector3.up * settings.ShoulderHeight
				- forward * settings.FollowDistance
				+ right * settings.ShoulderOffset;
		}

		private static UnityVector3 ComputeLookTarget(UnityVector3 playerPosition, float yawDegrees, SettingsConfig settings)
		{
			var yawRadians = yawDegrees * UnityEngine.Mathf.Deg2Rad;
			var forward = new UnityVector3(UnityEngine.Mathf.Sin(yawRadians), 0f, UnityEngine.Mathf.Cos(yawRadians));
			return playerPosition
				+ UnityVector3.up * settings.LookHeight
				+ forward * settings.LookAhead;
		}

		private static float NormalizePitch(float pitchDegrees)
		{
			return pitchDegrees > 180f ? pitchDegrees - 360f : pitchDegrees;
		}

		public sealed class SettingsConfig
		{
			public float FollowDistance = 5f;
			public float ShoulderHeight = 2.2f;
			public float ShoulderOffset = 0.65f;
			public float LookHeight = 1.4f;
			public float LookAhead = 2f;
			public float YawSmoothTime = 0.18f;
			public float PositionSmoothTime = 0.12f;
			public float RotationSmoothTime = 0.1f;

			public static SettingsConfig Default { get; } = new();
		}
	}
}
