using UnityEngine;

namespace Game.UnityBridge.Bootstrap
{
	public sealed class TerrainDemoBootstrap : MonoBehaviour
	{
		[SerializeField] private TerrainDemoSettings _settings = new();
		[SerializeField] private Material _terrainMaterial;
		[SerializeField] private Camera _camera;

		private void Awake()
		{
			TerrainDemoComposition.Build(_settings, transform, _terrainMaterial, _camera);
		}
	}
}
