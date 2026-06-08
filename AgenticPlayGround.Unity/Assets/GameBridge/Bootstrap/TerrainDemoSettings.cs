using System;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	[Serializable]
	public sealed class TerrainDemoSettings
	{
		[Header("World generation")]
		public int MapWidth = 64;
		public int MapHeight = 48;
		public int Seed = 42;
		public float FillProbability = 0.48f;
		public int CellularAutomataIterations = 5;
		public int MaxAttempts = 50;
		public int WaterPoolAttempts = 12;
		public int WaterPoolMaxSize = 5;

		[Header("Terrain mesh")]
		public float WorldUnitsPerTile = 1f;
		public float HeightScale = 1f;
		public float GroundHeight = 0f;
		public float WallHeight = 1f;
		public float WaterHeight = -1f;
		public float BevelInset = 0.3f;
		public int BevelSegments = 4;

		[Header("Player")]
		public float GroundSpeed = 4f;
		public float SwimSpeed = 2.5f;
		public float CharacterHalfHeight = 0.5f;
		public float TurnSpeedDegrees = 180f;

		[Header("Over-shoulder camera")]
		public float CameraFollowDistance = 5f;
		public float CameraShoulderHeight = 2.2f;
		public float CameraShoulderOffset = 0.65f;
		public float CameraLookHeight = 1.4f;
		public float CameraLookAhead = 2f;
		public float CameraYawSmoothTime = 0.18f;
		public float CameraPositionSmoothTime = 0.12f;
		public float CameraRotationSmoothTime = 0.1f;
	}
}
