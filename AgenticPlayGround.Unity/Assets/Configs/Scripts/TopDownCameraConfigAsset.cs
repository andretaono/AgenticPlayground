using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Presentation;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "TopDownCameraConfig", menuName = ConfigAssetMenus.Camera)]
	public sealed class TopDownCameraConfigAsset : ScriptableObject
	{
		[SerializeField] float orbitDistance = 16f;
		[SerializeField] float pitchDegrees = 52f;
		[SerializeField] float lookHeight = 0.75f;
		[SerializeField] float lookAhead = 0.25f;
		[SerializeField] float yawSmoothTime = 0.15f;
		[SerializeField] float positionSmoothTime = 0.1f;
		[SerializeField] float rotationSmoothTime = 0.08f;

		public TopDownCameraConfig ToConfig() =>
			new()
			{
				OrbitDistance = orbitDistance,
				PitchDegrees = pitchDegrees,
				LookHeight = lookHeight,
				LookAhead = lookAhead,
				YawSmoothTime = yawSmoothTime,
				PositionSmoothTime = positionSmoothTime,
				RotationSmoothTime = rotationSmoothTime
			};

		public void ApplyCodeDefaults()
		{
			var defaults = GameSessionDefaults.Default.Camera;
			orbitDistance = defaults.OrbitDistance;
			pitchDegrees = defaults.PitchDegrees;
			lookHeight = defaults.LookHeight;
			lookAhead = defaults.LookAhead;
			yawSmoothTime = defaults.YawSmoothTime;
			positionSmoothTime = defaults.PositionSmoothTime;
			rotationSmoothTime = defaults.RotationSmoothTime;
		}
	}
}
