using System;
using Game.Systems.Integration.Bootstrap;
using Game.Systems.Integration.TerrainMesh;
using UnityEngine;

namespace Game.UnityBridge.Configs
{
	[CreateAssetMenu(fileName = "TerrainConfig", menuName = ConfigAssetMenus.Terrain)]
	public sealed class TerrainConfigAsset : ScriptableObject
	{
		[SerializeField] float worldUnitsPerTile = 1f;
		[SerializeField] float heightScale = 1f;
		[SerializeField] TileHeightSettings heights = TileHeightSettings.CreateDefault();
		[SerializeField] TileSurfaceSettings surfaceMesh = TileSurfaceSettings.CreateDefault();

		public TerrainConfig ToConfig() =>
			new()
			{
				WorldUnitsPerTile = worldUnitsPerTile,
				HeightScale = heightScale,
				Heights = heights.ToConfig(),
				SurfaceMesh = surfaceMesh.ToConfig()
			};

		public void ApplyCodeDefaults()
		{
			var defaults = GameSessionDefaults.Default.Terrain;
			worldUnitsPerTile = defaults.WorldUnitsPerTile;
			heightScale = defaults.HeightScale;
			heights = TileHeightSettings.From(defaults.Heights);
			surfaceMesh = TileSurfaceSettings.From(defaults.SurfaceMesh);
		}

		[Serializable]
		public struct TileHeightSettings
		{
			public float GroundHeight;
			public float WallHeight;
			public float WaterHeight;

			public static TileHeightSettings CreateDefault() =>
				From(GameSessionDefaults.Default.Terrain.Heights);

			public static TileHeightSettings From(TileHeightModifierSettings settings) =>
				new()
				{
					GroundHeight = settings.GroundHeight,
					WallHeight = settings.WallHeight,
					WaterHeight = settings.WaterHeight
				};

			public TileHeightModifierSettings ToConfig() =>
				new()
				{
					GroundHeight = GroundHeight,
					WallHeight = WallHeight,
					WaterHeight = WaterHeight
				};
		}

		[Serializable]
		public struct TileSurfaceSettings
		{
			public bool EnableNormalSmoothing;
			public bool EnableStructuralNormalSmoothing;
			public float UpHardNormalThreshold;
			public float SoftNormalMinFaceDot;
			public float WeldEpsilon;
			public bool EnableGeometrySmoothing;
			public int GeometrySmoothDivisions;
			public float GeometrySmoothStrength;
			public bool EnableGroundGeometrySmoothing;
			public bool EnableStructuralGeometrySmoothing;

			public static TileSurfaceSettings CreateDefault() =>
				From(GameSessionDefaults.Default.Terrain.SurfaceMesh);

			public static TileSurfaceSettings From(TileSurfaceMeshSettings settings) =>
				new()
				{
					EnableNormalSmoothing = settings.EnableNormalSmoothing,
					EnableStructuralNormalSmoothing = settings.EnableStructuralNormalSmoothing,
					UpHardNormalThreshold = settings.UpHardNormalThreshold,
					SoftNormalMinFaceDot = settings.SoftNormalMinFaceDot,
					WeldEpsilon = settings.WeldEpsilon,
					EnableGeometrySmoothing = settings.EnableGeometrySmoothing,
					GeometrySmoothDivisions = settings.GeometrySmoothDivisions,
					GeometrySmoothStrength = settings.GeometrySmoothStrength,
					EnableGroundGeometrySmoothing = settings.EnableGroundGeometrySmoothing,
					EnableStructuralGeometrySmoothing = settings.EnableStructuralGeometrySmoothing
				};

			public TileSurfaceMeshSettings ToConfig() =>
				new()
				{
					EnableNormalSmoothing = EnableNormalSmoothing,
					EnableStructuralNormalSmoothing = EnableStructuralNormalSmoothing,
					UpHardNormalThreshold = UpHardNormalThreshold,
					SoftNormalMinFaceDot = SoftNormalMinFaceDot,
					WeldEpsilon = WeldEpsilon,
					EnableGeometrySmoothing = EnableGeometrySmoothing,
					GeometrySmoothDivisions = GeometrySmoothDivisions,
					GeometrySmoothStrength = GeometrySmoothStrength,
					EnableGroundGeometrySmoothing = EnableGroundGeometrySmoothing,
					EnableStructuralGeometrySmoothing = EnableStructuralGeometrySmoothing
				};
		}
	}
}
