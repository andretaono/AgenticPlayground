using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public static class TerrainMaterialFactory
	{
		public static Material CreateDefault()
		{
			var shader = FindVertexColorShader();
			return new Material(shader);
		}

		public static Material CreateVertexColorUnlit()
		{
			return new Material(FindVertexColorShader());
		}

		private static UnityEngine.Shader FindVertexColorShader()
		{
			var shader = UnityEngine.Shader.Find("GameBridge/VertexColorUnlit");
			if (shader == null)
				shader = UnityEngine.Shader.Find("Universal Render Pipeline/Lit");
			if (shader == null)
				shader = UnityEngine.Shader.Find("Standard");

			return shader;
		}
	}
}
