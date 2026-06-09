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
		public bool EnableCeilingLayer = true;
		public int MinWallBlobSize = 25;
		public int MinCaveAreaSize = 3;
		public int MaxCaveAreaSize = 49;
		public int MinCaveEntrances = 1;
		public int MaxCaveEntrances = 2;
		public int MinEntranceWidth = 1;
		public int MaxEntranceWidth = 3;
		public int MinEntranceDepth = 1;
		public int MaxEntranceDepth = 8;
		public int MaxCaveCount = 4;
		public int MaxCavesPerBlob = 3;
		[Range(0f, 1f)] public float ExtraWallStackChance = 0.15f;
		[Range(0f, 1f)] public float ExtraWallStackClusterChance = 0.75f;
		public int ExtraWallStackGrowPasses = 4;
		public int StartCeilingClearanceRadius = 4;

		[Header("Terrain")]
		public float WorldUnitsPerTile = 1f;
		public float HeightScale = 1f;
		public float GroundHeight = 0f;
		public float WallHeight = 1f;
		public float WaterHeight = -1f;

		[Header("Layer debug")]
		public bool EnableLayerDebug = true;
		public KeyCode DebugGroundKey = KeyCode.Alpha1;
		public KeyCode DebugOffKey = KeyCode.Alpha0;

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
