using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Enemies.PolarBear;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "PolarBearConfig", menuName = ConfigAssetMenus.PolarBear)]
	public sealed class PolarBearConfigAsset : ScriptableObject
	{
		[SerializeField] float directSightRange = 96f;
		[SerializeField] float longRangeScentRadius = 480f;
		[SerializeField] float scentDetectionThreshold = 0.5f;
		[SerializeField] float stalkMinDistance = 12f;
		[SerializeField] float stalkMaxDistance = 48f;
		[SerializeField] float vulnerableHealthThreshold = 60f;
		[SerializeField] float vulnerablePresenceThreshold = 8f;
		[SerializeField] float cognitionCellSize = 32f;
		[SerializeField] int cognitionGridWidth = 64;
		[SerializeField] int cognitionGridHeight = 64;
		[SerializeField] float groundSpeed = 1.5f;
		[SerializeField] float swimSpeed = 1f;
		[SerializeField] float maxHealth = 50f;

		public PolarBearConfig ToConfig() =>
			new()
			{
				DirectSightRange = directSightRange,
				LongRangeScentRadius = longRangeScentRadius,
				ScentDetectionThreshold = scentDetectionThreshold,
				StalkMinDistance = stalkMinDistance,
				StalkMaxDistance = stalkMaxDistance,
				VulnerableHealthThreshold = vulnerableHealthThreshold,
				VulnerablePresenceThreshold = vulnerablePresenceThreshold,
				CognitionCellSize = cognitionCellSize,
				CognitionGridWidth = cognitionGridWidth,
				CognitionGridHeight = cognitionGridHeight,
				GroundSpeed = groundSpeed,
				SwimSpeed = swimSpeed,
				MaxHealth = maxHealth
			};

		public void ApplyCodeDefaults()
		{
			var defaults = GameSessionDefaults.Default.PolarBear;
			directSightRange = defaults.DirectSightRange;
			longRangeScentRadius = defaults.LongRangeScentRadius;
			scentDetectionThreshold = defaults.ScentDetectionThreshold;
			stalkMinDistance = defaults.StalkMinDistance;
			stalkMaxDistance = defaults.StalkMaxDistance;
			vulnerableHealthThreshold = defaults.VulnerableHealthThreshold;
			vulnerablePresenceThreshold = defaults.VulnerablePresenceThreshold;
			cognitionCellSize = defaults.CognitionCellSize;
			cognitionGridWidth = defaults.CognitionGridWidth;
			cognitionGridHeight = defaults.CognitionGridHeight;
			groundSpeed = defaults.GroundSpeed;
			swimSpeed = defaults.SwimSpeed;
			maxHealth = defaults.MaxHealth;
		}
	}
}
