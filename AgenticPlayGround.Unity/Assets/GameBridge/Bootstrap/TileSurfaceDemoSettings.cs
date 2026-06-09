using System;
using Game.Systems.Integration.TerrainMesh;
using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	[Serializable]
	public sealed class TileSurfaceDemoSettings
	{
		public bool EnableNormalSmoothing = true;
		public bool EnableStructuralNormalSmoothing = true;
		[Range(0.5f, 1f)] public float UpHardNormalThreshold = 0.9f;
		[Range(1e-5f, 1e-3f)] public float WeldEpsilon = 1e-4f;

		public TileSurfaceMeshSettings ToIntegrationSettings() => new()
		{
			EnableNormalSmoothing = EnableNormalSmoothing,
			EnableStructuralNormalSmoothing = EnableStructuralNormalSmoothing,
			UpHardNormalThreshold = UpHardNormalThreshold,
			WeldEpsilon = WeldEpsilon
		};
	}
}
