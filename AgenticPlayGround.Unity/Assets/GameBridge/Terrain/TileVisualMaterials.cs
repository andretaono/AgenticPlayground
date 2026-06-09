using Game.Systems.Domain.World.Model;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public static class TileVisualMaterials
	{
		private static Material _groundMaterial;
		private static Material _caveGroundMaterial;
		private static Material _wallMaterial;
		private static Material _waterMaterial;
		private static Material _ceilingMaterial;

		public static Material GetGroundMaterial() =>
			_groundMaterial ??= CreateMaterial(new Color(0.76f, 0.70f, 0.50f));

		public static Material GetCaveGroundMaterial() =>
			_caveGroundMaterial ??= CreateMaterial(new Color(0.55f, 0.5f, 0.3f));

		public static Material GetWallMaterial() =>
			_wallMaterial ??= CreateMaterial(new Color(0.40f, 0.40f, 0.45f));

		public static Material GetWaterMaterial() =>
			_waterMaterial ??= CreateMaterial(new Color(0.20f, 0.40f, 0.80f));

		public static Material GetCeilingMaterial() =>
			_ceilingMaterial ??= CreateMaterial(new Color(0.52f, 0.52f, 0.56f));

		private static Material CreateMaterial(Color color)
		{
			var shader = Shader.Find("Universal Render Pipeline/Lit")
			             ?? Shader.Find("Standard");
			var material = new Material(shader) { color = color };
			return material;
		}
	}
}
