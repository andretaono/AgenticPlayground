using System;
using Game.Systems.Integration.Player;

namespace Game.UnityBridge.Bootstrap
{
	[Serializable]
	public sealed class PlayerDemoSettings
	{
		public float GroundSpeed = 4f;
		public float SwimSpeed = 2.5f;
		public float CharacterHalfHeight = 0.5f;
		public float TurnSpeedDegrees = 180f;

		public PlayerConfig ToPlayerConfig() => new()
		{
			GroundSpeed = GroundSpeed,
			SwimSpeed = SwimSpeed
		};
	}
}
