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

		[Header("Polar bears")]
		public int MinPolarBearCount = 1;
		public int MaxPolarBearCount = 3;

		[Header("Terrain")]
		public float WorldUnitsPerTile = 1f;
		public float HeightScale = 1f;
		public float GroundHeight = 0f;
		public float WallHeight = 1f;
		public float WaterHeight = -1f;
		public TileSurfaceDemoSettings SurfaceMesh = new();

		[Header("Layer debug")]
		public bool EnableLayerDebug = true;
		public KeyCode DebugGroundKey = KeyCode.Alpha1;
		public KeyCode DebugOffKey = KeyCode.Alpha0;

		[Header("Player")]
		public PlayerDemoSettings Player = new();

		[Header("Top-down camera")]
		public float CameraOrbitDistance = 16f;
		[Range(25f, 80f)] public float CameraPitchDegrees = 52f;
		public float CameraLookHeight = 0.75f;
		public float CameraLookAhead = 0.25f;
		public float CameraYawSmoothTime = 0.15f;
		public float CameraPositionSmoothTime = 0.1f;
		public float CameraRotationSmoothTime = 0.08f;
	}
}
