using UnityEngine;

namespace Game.UnityBridge.Terrain
{
	public static class TerrainMaterialFactory
	{
		public static Material CreateDefault()
		{
			var shader = Shader.Find("GameBridge/VertexColorUnlit");
			if (shader == null)
				shader = Shader.Find("Universal Render Pipeline/Lit");
			if (shader == null)
				shader = Shader.Find("Standard");

			return new Material(shader);
		}
	}
}
