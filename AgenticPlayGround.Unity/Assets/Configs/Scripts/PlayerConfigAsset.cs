using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.Player;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "PlayerConfig", menuName = ConfigAssetMenus.Player)]
	public sealed class PlayerConfigAsset : ScriptableObject
	{
		[SerializeField] float groundSpeed = 4f;
		[SerializeField] float swimSpeed = 2.5f;
		[SerializeField] float bodyRadius = 0.4f;
		[SerializeField] float maxHealth = 100f;
		[SerializeField] float turnSpeedDegrees = 180f;
		[SerializeField] float characterHalfHeight = 0.5f;

		public PlayerConfig ToConfig() =>
			new()
			{
				GroundSpeed = groundSpeed,
				SwimSpeed = swimSpeed,
				BodyRadius = bodyRadius,
				MaxHealth = maxHealth,
				TurnSpeedDegrees = turnSpeedDegrees,
				CharacterHalfHeight = characterHalfHeight
			};

		public void ApplyCodeDefaults()
		{
			var defaults = GameSessionDefaults.Default.Player;
			groundSpeed = defaults.GroundSpeed;
			swimSpeed = defaults.SwimSpeed;
			bodyRadius = defaults.BodyRadius;
			maxHealth = defaults.MaxHealth;
			turnSpeedDegrees = defaults.TurnSpeedDegrees;
			characterHalfHeight = defaults.CharacterHalfHeight;
		}
	}
}
