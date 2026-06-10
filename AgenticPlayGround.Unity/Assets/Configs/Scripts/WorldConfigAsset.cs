using System;
using Game.Systems.Domain.World.Generation.Model;
using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.World;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "WorldConfig", menuName = ConfigAssetMenus.World)]
	public sealed class WorldConfigAsset : ScriptableObject
	{
		[SerializeField] WorldGenerationSettings generation = WorldGenerationSettings.CreateDefault();

		public WorldConfig ToConfig() =>
			new() { Generation = generation.ToConfig() };

		public void ApplyCodeDefaults() =>
			generation = WorldGenerationSettings.From(GameSessionDefaults.Default.World.Generation);

		[Serializable]
		public struct WorldGenerationSettings
		{
			public int Width;
			public int Height;
			public int Seed;
			[Range(0f, 1f)] public float FillProbability;
			public int CellularAutomataIterations;
			public int MaxAttempts;
			public int WaterPoolAttempts;
			public int WaterPoolMaxSize;
			public bool EnableCeilingLayer;
			public int MinWallBlobSize;
			public int MinCaveAreaSize;
			public int MaxCaveAreaSize;
			public int MinCaveEntrances;
			public int MaxCaveEntrances;
			public int MinEntranceWidth;
			public int MaxEntranceWidth;
			public int MinEntranceDepth;
			public int MaxEntranceDepth;
			public int MaxCaveCount;
			public int MaxCavesPerBlob;
			[Range(0f, 1f)] public float ExtraWallStackChance;
			[Range(0f, 1f)] public float ExtraWallStackClusterChance;
			public int ExtraWallStackGrowPasses;
			public int StartCeilingClearanceRadius;

			public static WorldGenerationSettings CreateDefault() =>
				From(GameSessionDefaults.Default.World.Generation);

			public static WorldGenerationSettings From(WorldGenerationConfig config) =>
				new()
				{
					Width = config.Width,
					Height = config.Height,
					Seed = config.Seed,
					FillProbability = config.FillProbability,
					CellularAutomataIterations = config.CellularAutomataIterations,
					MaxAttempts = config.MaxAttempts,
					WaterPoolAttempts = config.WaterPoolAttempts,
					WaterPoolMaxSize = config.WaterPoolMaxSize,
					EnableCeilingLayer = config.EnableCeilingLayer,
					MinWallBlobSize = config.MinWallBlobSize,
					MinCaveAreaSize = config.MinCaveAreaSize,
					MaxCaveAreaSize = config.MaxCaveAreaSize,
					MinCaveEntrances = config.MinCaveEntrances,
					MaxCaveEntrances = config.MaxCaveEntrances,
					MinEntranceWidth = config.MinEntranceWidth,
					MaxEntranceWidth = config.MaxEntranceWidth,
					MinEntranceDepth = config.MinEntranceDepth,
					MaxEntranceDepth = config.MaxEntranceDepth,
					MaxCaveCount = config.MaxCaveCount,
					MaxCavesPerBlob = config.MaxCavesPerBlob,
					ExtraWallStackChance = config.ExtraWallStackChance,
					ExtraWallStackClusterChance = config.ExtraWallStackClusterChance,
					ExtraWallStackGrowPasses = config.ExtraWallStackGrowPasses,
					StartCeilingClearanceRadius = config.StartCeilingClearanceRadius
				};

			public WorldGenerationConfig ToConfig() =>
				new()
				{
					Width = Width,
					Height = Height,
					Seed = Seed,
					FillProbability = FillProbability,
					CellularAutomataIterations = CellularAutomataIterations,
					MaxAttempts = MaxAttempts,
					WaterPoolAttempts = WaterPoolAttempts,
					WaterPoolMaxSize = WaterPoolMaxSize,
					EnableCeilingLayer = EnableCeilingLayer,
					MinWallBlobSize = MinWallBlobSize,
					MinCaveAreaSize = MinCaveAreaSize,
					MaxCaveAreaSize = MaxCaveAreaSize,
					MinCaveEntrances = MinCaveEntrances,
					MaxCaveEntrances = MaxCaveEntrances,
					MinEntranceWidth = MinEntranceWidth,
					MaxEntranceWidth = MaxEntranceWidth,
					MinEntranceDepth = MinEntranceDepth,
					MaxEntranceDepth = MaxEntranceDepth,
					MaxCaveCount = MaxCaveCount,
					MaxCavesPerBlob = MaxCavesPerBlob,
					ExtraWallStackChance = ExtraWallStackChance,
					ExtraWallStackClusterChance = ExtraWallStackClusterChance,
					ExtraWallStackGrowPasses = ExtraWallStackGrowPasses,
					StartCeilingClearanceRadius = StartCeilingClearanceRadius
				};
		}
	}
}
