using System;
using Game.Systems.Integration.TerrainMesh;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	[Serializable]
	public sealed class TileSurfaceDemoSettings
	{
		[Header("Normal smoothing")]
		public bool EnableNormalSmoothing = true;
		public bool EnableStructuralNormalSmoothing = true;
		[Tooltip("Minimum alignment between softened and face normals. 0 disables.")]
		[Range(0f, 1f)] public float SoftNormalMinFaceDot = 0.6f;

		[Header("Geometry smoothing")]
		[Tooltip("Master toggle. Divisions and strength have no effect when this is off. Applied when Play starts.")]
		public bool EnableGeometrySmoothing = false;
		public bool EnableStructuralGeometrySmoothing = true;
		public bool EnableGroundGeometrySmoothing = true;
		[Tooltip("Splits per face edge. 1 = 2×2 quads, 2 = 3×3.")]
		[Range(0, 3)] public int GeometrySmoothDivisions = 1;
		[Range(0f, 1f)] public float GeometrySmoothStrength = 0.35f;

		[Header("Shared")]
		[Range(0.5f, 1f)] public float UpHardNormalThreshold = 0.9f;
		[Range(1e-5f, 1e-3f)] public float WeldEpsilon = 1e-4f;

		public TileSurfaceMeshSettings ToIntegrationSettings() => new()
		{
			EnableNormalSmoothing = EnableNormalSmoothing,
			EnableStructuralNormalSmoothing = EnableStructuralNormalSmoothing,
			SoftNormalMinFaceDot = SoftNormalMinFaceDot,
			EnableGeometrySmoothing = EnableGeometrySmoothing,
			EnableStructuralGeometrySmoothing = EnableStructuralGeometrySmoothing,
			EnableGroundGeometrySmoothing = EnableGroundGeometrySmoothing,
			GeometrySmoothDivisions = GeometrySmoothDivisions,
			GeometrySmoothStrength = GeometrySmoothStrength,
			UpHardNormalThreshold = UpHardNormalThreshold,
			WeldEpsilon = WeldEpsilon
		};
	}
}
