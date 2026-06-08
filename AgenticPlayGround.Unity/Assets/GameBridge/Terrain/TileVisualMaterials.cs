using Game.Systems.Domain.World.Model;
using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public static class TileVisualMaterials
	{
		private static Material _groundMaterial;
		private static Material _wallMaterial;
		private static Material _waterMaterial;

		public static Material GetGroundMaterial() =>
			_groundMaterial ??= CreateMaterial(new Color(0.76f, 0.70f, 0.50f));

		public static Material GetWallMaterial() =>
			_wallMaterial ??= CreateMaterial(new Color(0.40f, 0.40f, 0.45f));

		public static Material GetWaterMaterial() =>
			_waterMaterial ??= CreateMaterial(new Color(0.20f, 0.40f, 0.80f));

		private static Material CreateMaterial(Color color)
		{
			var shader = Shader.Find("Universal Render Pipeline/Lit")
			             ?? Shader.Find("Standard");
			var material = new Material(shader) { color = color };
			return material;
		}
	}
}
