using System;
using Game.Systems.Integration.Player;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	[Serializable]
	public sealed class PlayerDemoSettings
	{
		public float GroundSpeed = 4f;
		public float SwimSpeed = 2.5f;
		public float CharacterHalfHeight = 0.5f;
		[Tooltip("Horizontal collision radius in tile units.")]
		public float CharacterRadius = 0.4f;
		public float TurnSpeedDegrees = 180f;
		public float MaxHealth = 100f;

		public PlayerConfig ToPlayerConfig() => new()
		{
			GroundSpeed = GroundSpeed,
			SwimSpeed = SwimSpeed,
			BodyRadius = CharacterRadius,
			MaxHealth = MaxHealth
		};
	}
}
